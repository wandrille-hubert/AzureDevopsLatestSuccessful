using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;

namespace ADOLatestSuccessful
{
    class RestHelper
    {
        private ReleaseHttpClient relclient;
        private BuildHttpClient buildclient;
        private ProjectHttpClient projclient;
        string personalaccesstoken;
        public TfsInfo TfsEnvInfo { get; internal set; }

        public RestHelper(string url, string pat, TfsInfo tfsinfo)
        {
            TfsEnvInfo = tfsinfo;

            // Initialize connection to azure devops
            personalaccesstoken = pat;
            var networkCredential = new VssBasicCredential(string.Empty, pat);
            VssConnection connection = new VssConnection(new Uri(url), networkCredential);
            relclient = connection.GetClient<ReleaseHttpClient>();
            buildclient = connection.GetClient<BuildHttpClient>();
            projclient = connection.GetClient<ProjectHttpClient>();

            // Initialize latest return entities
            Build latestBuild = new Build();
            Release latestRelease = new Release();
            Build latestBuildForSpecifiedDefinition = new Build();
            Release latestReleaseForSpecifiedDefinition = new Release();

            // Get latest successful build and release for a project
            latestBuild = GetLatestSuccessfulBuild();
            latestRelease = GetLatestSuccessfulRelease();

            Console.WriteLine("latest Successful Build:");
            Console.WriteLine("--------------------------");
            Console.WriteLine("id=" + latestBuild.Id);
            Console.WriteLine("starttime=" + latestBuild.StartTime);
            Console.WriteLine("finishtime=" + latestBuild.FinishTime);
            Console.WriteLine();
            Console.WriteLine("latest Successful Release:");
            Console.WriteLine("--------------------------");
            Console.WriteLine("id=" + latestRelease.Id);
            Console.WriteLine("modifiedon=" + latestRelease.ModifiedOn);
            Console.WriteLine();
            Console.WriteLine();

            // Get latest successful build and release for a project/specified build/release definition id
            latestBuildForSpecifiedDefinition = GetLatestSuccessfulBuild(TfsEnvInfo.BuildDefinitionId);
            latestReleaseForSpecifiedDefinition = GetLatestSuccessfulRelease(true);

            Console.WriteLine("latest Successful Build For Specified Definition:");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("id=" + latestBuildForSpecifiedDefinition.Id);
            Console.WriteLine("starttime=" + latestBuildForSpecifiedDefinition.StartTime);
            Console.WriteLine("finishtime=" + latestBuildForSpecifiedDefinition.FinishTime);
            Console.WriteLine();
            Console.WriteLine("latest Successful Release For Specified Definition:");
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("id=" + latestReleaseForSpecifiedDefinition.Id);
            Console.WriteLine("modifiedon=" + latestReleaseForSpecifiedDefinition.ModifiedOn);
            Console.WriteLine();
            Console.WriteLine();
        }

        public Release GetLatestSuccessfulRelease()
        {
            Release retRelease = new Release();

            try
            {
                // get list of releases for a provided project name in a descending order
                var releases = relclient.GetReleasesAsync(project: TfsEnvInfo.ProjectName, expand: ReleaseExpands.Environments, queryOrder: ReleaseQueryOrder.Descending).Result;

                foreach (var release in releases)
                {
                    bool isLatestSuccess = true;

                    // assumption: if a release contains no environments, then "skip" it
                    if (release.Environments.Count < 1)
                    {
                        isLatestSuccess = false;
                    }

                    // cycle through all of the release's environments and check to see if it was successful
                    foreach (var env in release.Environments)
                    {
                        var releaseEnv = relclient.GetReleaseEnvironmentAsync(project: TfsEnvInfo.ProjectName, releaseId: release.Id, environmentId: env.Id).Result;

                        if (releaseEnv.Status != EnvironmentStatus.Succeeded)
                        {
                            isLatestSuccess = false;
                        }
                    }

                    // if all environments were successful, and since releases are in descending order
                    // then can say that this is the latest release
                    if (isLatestSuccess)
                    {
                        retRelease = release;
                        break;
                    }
                }

                return retRelease;
            }
            catch (Exception exc)
            {
                return null;
            }
        }

        public Release GetLatestSuccessfulRelease(bool specifiedDef)
        {
            Release retRelease = new Release();
            try
            {
                // get release definition for a provided project name and release definition name
                // this is in order to get the release definition id
                var def = relclient.GetReleaseDefinitionsAsync(TfsEnvInfo.ProjectName, TfsEnvInfo.ReleaseDefinitionName, isExactNameMatch: true).Result;

                if (def.Count() > 0)
                {
                    // get list of releases for a provided project name and release definition id in a descending order
                    var id = def.First().Id;
                    var releases = relclient.GetReleasesAsync(project: TfsEnvInfo.ProjectName, definitionId: id, expand: ReleaseExpands.Environments, queryOrder: ReleaseQueryOrder.Descending).Result;

                    foreach (var release in releases)
                    {
                        bool isLatestSuccess = true;

                        // assumption: if a release contains no environments, then "skip" it
                        if (release.Environments.Count < 1)
                        {
                            isLatestSuccess = false;
                        }

                        // cycle through all of the release's environments and check to see if it was successful
                        foreach (var env in release.Environments)
                        {
                            var releaseEnv = relclient.GetReleaseEnvironmentAsync(project: TfsEnvInfo.ProjectName, releaseId: release.Id, environmentId: env.Id).Result;

                            if (releaseEnv.Status != EnvironmentStatus.Succeeded)
                            {
                                isLatestSuccess = false;
                            }
                        }

                        // if all environments were successful, and since releases are in descending order
                        // then can say that this is the latest release
                        if (isLatestSuccess)
                        {
                            retRelease = release;
                            break;
                        }
                    }
                }

                return retRelease;
            }
            catch (Exception exc)
            {
                return null;
            }
        }

        public Build GetLatestSuccessfulBuild()
        {
            Build retBuild = new Build();
            try
            {
                // get release definition for a provided project name
                // this is in order to get the project id
                var proj = projclient.GetProject(TfsEnvInfo.ProjectName).Result;

                if (proj != null)
                {
                    // get list of successful builds for a provided project name in a descending order and only returning the top 1
                    var def = buildclient.GetBuildsAsync(proj.Id, resultFilter: BuildResult.Succeeded, queryOrder: BuildQueryOrder.StartTimeDescending, top: 1).Result;

                    if (def.Count() > 0)
                    {
                        var id = def.First().Id;
                        retBuild = def.First();
                    }
                }

                return retBuild;
            }
            catch (Exception exc)
            {
                return null;
            }
        }

        public Build GetLatestSuccessfulBuild(int buildDefId)
        {
            Build retBuild = new Build();

            try
            {
                // get release definition for a provided project name
                // this is in order to get the project id
                var proj = projclient.GetProject(TfsEnvInfo.ProjectName).Result;

                if (proj != null)
                {
                    IList<int> buildDefList = new List<int>();
                    buildDefList.Add(buildDefId);

                    // get list of successful builds for a provided project name and build definition id in a descending order and only returning the top 1
                    var def = buildclient.GetBuildsAsync(proj.Id, buildDefList, resultFilter: BuildResult.Succeeded, queryOrder: BuildQueryOrder.StartTimeDescending, top: 1).Result;

                    if (def.Count() > 0)
                    {
                        var id = def.First().Id;
                        retBuild = def.First();
                    }
                }
                

                return retBuild;
            }
            catch (Exception exc)
            {
                return null;
            }
        }

        public struct TfsInfo
        {
            public string ProjectCollectionUrl;
            public string ProjectName;
            public string ReleaseDefinitionName;
            public int ReleaseDefinitionID;
            public string ReleaseName;
            public string EnvironmentName;
            public int BuildDefinitionId;
        }
    }
}
