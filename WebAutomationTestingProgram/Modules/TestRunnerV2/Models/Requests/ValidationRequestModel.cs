using System.ComponentModel.DataAnnotations;
using WebAutomationTestingProgram.Core.Models.Attributes;

namespace WebAutomationTestingProgram.Modules.TestRunnerV2.Models.Requests
{
    /// <summary>
    /// Used by API Requests to create a ValidationRequest
    /// </summary>
    public class ValidationRequestModel
    {
        [Required(ErrorMessage = "A file must be provided.")]
        [ValidFile([".xls", ".xlsx", ".xlsm", ".csv", ".txt", ".json"])]
        public IFormFile File { get; set; }
    }
}
