using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Modules.TestRunner
{
    public class PauseRequestModel
    {
        [Required(ErrorMessage = "The ID is required")]
        [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "ID must be in correct format.")]
        public string ID { get; set; }
    }
}
