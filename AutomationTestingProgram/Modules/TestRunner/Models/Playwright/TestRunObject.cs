using AutomationTestingProgram.Core;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public class TestRunObject
    {
        /// <summary>
        /// The name of the Test Run
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The id of the test run.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The id of the test plan the run is part of.
        /// </summary>
        public int PlanID { get; set; }

        /// <summary>
        /// The id of the test suite the run is part of.
        /// </summary>
        public int SuiteID { get; set; }


        /// <summary>
        /// The total # of Test Cases in the whole run.
        /// </summary>
        public int TestCaseNum => TestCases.Count;

        /// <summary>
        /// List of all TestCases within this TestRun
        /// </summary>
        public IList<TestCaseObject> TestCases { get; }


        /// <summary>
        /// The result of the TestRun
        /// </summary>
        public Result Result { get; set; } = Result.NotExecuted;

        /// <summary>
        /// The Start Date of the TestRun.
        /// </summary>
        public DateTime StartedDate { get; set; }

        /// <summary>
        /// The Completed Date of the TestRun
        /// </summary>
        public DateTime CompletedDate { get; set; }

        /// <summary>
        /// Total # of failed steps in Test Case
        /// </summary>
        public int FailureCounter { get; set; }


        private CSVEnvironmentGetter _csvGetter;


        public TestRunObject(CSVEnvironmentGetter csvGetter, string name)
        {
            Name = name;
            FailureCounter = 0;
            TestCases = new List<TestCaseObject>();

            _csvGetter = csvGetter;
        }

        public string GenerateRunInformation(ProcessRequest request)
        {
            string fwVersion = "PLACEHOLDER";

            // run info is displayed as info for the run (note only for WINDOWS)
            string runInfoStr = string.Empty;
            runInfoStr += "### Run Info\n\n";
            runInfoStr += "**Automation Program Version:** " + fwVersion;

            runInfoStr += $"\n\n**Environment:** [{request.Environment}]({_csvGetter.GetOpsBpsURL(request.Environment)})";
            runInfoStr += $"\n\n**BuildNumber:** PLACEHOLDER";
            runInfoStr += $"\n\n**Browser:** {request.BrowserType}";
            runInfoStr += $"\n\n**Browser Version:** {request.BrowserVersion}";

            runInfoStr += "\n\n### Machine Info";

            OperatingSystem os_info = Environment.OSVersion;
            runInfoStr += "\n\n**OS Version:** Windows " + os_info.Version.Major;
            runInfoStr += $"\n\n**OS Build:** {os_info.Version.Build}";
            runInfoStr += $"\n\n**Processors:** {Environment.ProcessorCount}";

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == "C:\\")
                {
                    runInfoStr += $"\n\n**Free Space on C Drive:** " + $"{drive.TotalFreeSpace / ((1024 * 1024) * 1024)} GB";
                }
            }

            runInfoStr += "\n\n**Machine Name:** " + Environment.MachineName;
            runInfoStr += "\n\n**64 bit OS?:** " + Environment.Is64BitOperatingSystem.ToString();
            runInfoStr += "\n\n**Process Path:** " + Environment.ProcessPath;
            runInfoStr += "\n\n**Machine User Domain Name:** " + Environment.UserDomainName;
            runInfoStr += "\n\n**Machine User Name:** " + Environment.UserName;
            runInfoStr += "\n\n**User Interactive?:** " + Environment.UserInteractive.ToString();

            string testerEmail = request.User.Identity?.Name ?? "Unknown";
            
            runInfoStr += "\n\n**Tester:** " + testerEmail.Split('@')[0].Replace('.', ' ');

            /*List<string> emails = InformationObject.NotifyEmails ?? System.Configuration.ConfigurationManager.AppSettings["EMAILS_LIST"].Replace(" ", string.Empty).Split(",").ToList();
            if (emails != null)
            {
                runInfoStr += "\n\n**Notify List:** ";
                foreach (string email in emails)
                {
                    runInfoStr += $"[{email}](mailto:{email}) ";
                }
            }*/

            string teamEmail = "edu.l.csc.ddsb.qa.regression@msgov.gov.on.ca";

            runInfoStr += "\n\n**Tester Email:** " + $"[{testerEmail}](mailto:{testerEmail})";
            runInfoStr += $"\n\n**Framework Contact:** [edu.l.csc.ddsb.qa.regression@msgov.gov.on.ca](mailto:{teamEmail})";

            return runInfoStr;
        }
    }

    public enum Result
    {
        /// <summary>
        /// Step uncomplete - still processing/yet to process. Neither failed nor successful.
        /// </summary>
        NotExecuted,

        /// <summary>
        /// Step failed
        /// </summary>
        Failed,

        /// <summary>
        /// Step successful
        /// </summary>
        Completed,

        /// <summary>
        /// Step skipped
        /// </summary>
        Skipped,

        /// <summary>
        /// Step cancelled
        /// </summary>
        Cancelled,
    }
}
