using System.ComponentModel.DataAnnotations;
using WebAutomationTestingProgram.Core.Models.Attributes;
using WebAutomationTestingProgram.Modules.TestRunnerV2.Models.Attributes;

namespace WebAutomationTestingProgram.Modules.TestRunnerV2.Models.Requests
{
    /// <summary>
    /// Used by API Requests to create a ProcessRequest
    /// </summary>
    public class ProcessRequestModel
    {
        [Required(ErrorMessage = "A file must be provided. Allowed types: .xls, .xlsx, .xlsm")]
        [ValidFile([".xls", ".xlsx", ".xlsm"])]
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "A browser must be provided. Allowed browsers: Chrome, Edge, Firefox")]
        [ValidBrowser(["Chrome", "Edge", "Firefox"])]
        public string Browser { get; set; }

        [Required(ErrorMessage = "A browser version must be provided. Allowed Formats: 123, 123.0, 123.0.0, 123.0.0.0")]
        [RegularExpression(@"^\d+(\.\d+){0,3}$", ErrorMessage = "Version must be in the correct format. Allowed Formats: 123, 123.0, 123.0.0, 123.0.0.0")]
        public string BrowserVersion { get; set; }

        [Required(ErrorMessage = "The environment is required.")]
        [ValidEnvironment()]
        public string Environment { get; set; }

        [Required(ErrorMessage = "The delay is required.")]
        public double Delay { get; set; }
    }
}
