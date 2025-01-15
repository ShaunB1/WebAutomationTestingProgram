using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Core
{
    public class AllowedFilterTypeAttribute : ValidationAttribute
    {
        public AllowedFilterTypeAttribute() { }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string type)
            {
                if (!Enum.TryParse(type, out FilterType _))
                {
                    return new ValidationResult($"Filter Type '{value}' is not valid.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
