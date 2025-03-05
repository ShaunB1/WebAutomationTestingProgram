using System.ComponentModel.DataAnnotations;

namespace WebAutomationTestingProgram.Modules.TestRunnerV2.Models.Requests
{
    public class PasswordResetRequestModel
    {
        [Required(ErrorMessage = "The Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}
