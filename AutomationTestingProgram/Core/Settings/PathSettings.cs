namespace AutomationTestingProgram.Core
{
    public class PathSettings
    {
        public string ExtensionDownloadPath { get; set; }
        public string KeychainFilePath { get; set; }
        public string EnvironmentsListPath { get; set; }
        public string ActionsPath { get; set; }

        public class Script
        {
            public string PR_DELETE { get; set; }
            public string PR_REVERT { get; set; }
        }

        public Script Scripts { get; set; }
    }
}
