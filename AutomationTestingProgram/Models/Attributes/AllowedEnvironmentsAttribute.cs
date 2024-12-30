using AutomationTestingProgram.Helper;
using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Models.Attributes
{
    public class AllowedEnvironmentsAttribute : ValidationAttribute
    {
        public AllowedEnvironmentsAttribute() { }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string environment)
            {
                try
                {
                    CSVEnvironmentGetter.GetEnvironmentName(environment);
                }
                catch
                {
                    return new ValidationResult($"Environment '{environment}' is not valid.");
                }
            }

            return ValidationResult.Success!;
        }
    }
}
