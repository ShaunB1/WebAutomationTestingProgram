using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Models
{
    /// <summary>
    /// Used by API Requests to create a ValidationRequest
    /// </summary>
    public class ValidationRequestModel
    {
        [Required(ErrorMessage = "A file must be provided.")]
        [AllowedFileExtensions(new[] { ".xls", ".xlsx", ".xlsm", ".csv", ".txt", ".json" })]
        public IFormFile File { get; set; }
    }
}
