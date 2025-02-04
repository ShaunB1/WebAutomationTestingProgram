using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public class SecretKeyRetrievalRequestModel
    {
        [Required(ErrorMessage = "The Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}
