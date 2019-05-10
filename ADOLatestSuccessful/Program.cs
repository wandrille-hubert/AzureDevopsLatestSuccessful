using System;
using static ADOLatestSuccessful.RestHelper;

namespace ADOLatestSuccessful
{
    class Program
    {
        public TfsInfo TfsEnvInfo { get; internal set; }
        static void Main(string[] args)
        {
            // TODO: Update these values as needed
            TfsInfo tfsEnvInfo = new TfsInfo();
            tfsEnvInfo.ProjectName = "YOURPROJECTNAME";
            tfsEnvInfo.ReleaseDefinitionName = "YOUR_RELEASE_DEFINITION_NAME";
            tfsEnvInfo.BuildDefinitionId = 10; //YOUR_BUILD_DEFINITION_ID

            //Console.SetWindowSize(Console.WindowWidth * 2, Console.WindowHeight);

            // TODO: Update these values as needed
            const string tfsUrl = "YOUR_BASE_URL, for example https://accountname.visualstudio.com/";
            const string personalaccesstoken = "YOUR_PERSONAL_ACCESS_TOKEN";

            var tfsWork = new RestHelper(tfsUrl, personalaccesstoken, tfsEnvInfo);

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
