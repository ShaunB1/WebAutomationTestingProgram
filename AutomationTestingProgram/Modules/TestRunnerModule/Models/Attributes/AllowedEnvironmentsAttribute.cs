using AutomationTestingProgram.Core;
using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public class AllowedEnvironmentsAttribute : ValidationAttribute
    {
        public AllowedEnvironmentsAttribute() { }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string environment)
            {
                var csvEnvironmentGetter = (CSVEnvironmentGetter)validationContext.GetService(typeof(CSVEnvironmentGetter))!;

                try
                {
                    csvEnvironmentGetter.GetEnvironmentName(environment);
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
