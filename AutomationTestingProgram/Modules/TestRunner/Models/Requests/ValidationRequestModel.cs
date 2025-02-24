using System.ComponentModel.DataAnnotations;
using AutomationTestingProgram.Core;
using AutomationTestingProgram.Core.Models.Attributes;

namespace AutomationTestingProgram.Modules.TestRunnerModule
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
