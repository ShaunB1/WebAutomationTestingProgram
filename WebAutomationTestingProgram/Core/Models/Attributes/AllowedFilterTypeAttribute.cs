using System.ComponentModel.DataAnnotations;
using WebAutomationTestingProgram.Core.Requests;

namespace WebAutomationTestingProgram.Core.Models.Attributes
{
    public class AllowedFilterTypeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string type) return ValidationResult.Success;
            return !Enum.TryParse(type, out FilterType _) ? new ValidationResult($"Filter Type '{value}' is not valid.") : ValidationResult.Success;
        }
    }
}
