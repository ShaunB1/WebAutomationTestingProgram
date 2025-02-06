using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Core
{
    /// <summary>
    /// Used by API requests to create a RetrieveLogFileRequest
    /// </summary>
    public class RetrieveLogFileModel
    {
        [Required(ErrorMessage = "The ID is required")]
        [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "ID must be in correct format.")]
        public string ID { get; set; }
    }
}
