using AutomationTestingProgram.Models.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Models
{
    /// <summary>
    /// Used by API Requests to create a ProcessRequest
    /// </summary>
    public class ProcessRequestModel
    {   
        [Required(ErrorMessage = "A file must be provided.")]
        [AllowedFileExtensions(new[] { ".xls", ".xlsx", ".xlsm", ".csv", ".txt", ".json" })]
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "The browser type is required.")]
        [AllowedBrowserType(new[] { "Chrome", "Edge", "Firefox", "RemoteChrome", "RemoteEdge", "RemoteFirefox"})]
        public string Type { get; set; }

        [Required(ErrorMessage = "The version is required.")]
        [RegularExpression(@"^\d+(\.\d+){0,3}$", ErrorMessage = "Version must be in correct format.")]
        public string Version { get; set; }

        [Required(ErrorMessage = "The environment is required.")]
        [AllowedEnvironments()]
        public string Environment { get; set; }
    }
}
