using System.ComponentModel.DataAnnotations;
using AutomationTestingProgram.Core.Services;

namespace AutomationTestingProgram.Modules.TestRunner.Models.Attributes
{
    public class ValidEnvironmentAttribute : ValidationAttribute
    {
        private readonly List<string> _environments;
        
        public ValidEnvironmentAttribute()
        {
            _environments = new List<string>();
        }
        
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string environment)
            {
                return new ValidationResult("Invalid input. Input must be a string.");
            }

            var environmentGetter = validationContext.GetService(typeof(CsvEnvironmentGetter)) as CsvEnvironmentGetter;

            // _environments = environmentGetter.GetEnvironments();
            
            
            // if (value is string environment)
            // {
            //     var csvEnvironmentGetter = (CSVEnvironmentGetter)validationContext.GetService(typeof(CSVEnvironmentGetter))!;
            //
            //     try
            //     {
            //         csvEnvironmentGetter.GetEnvironmentName(environment);
            //     }
            //     catch
            //     {
            //         return new ValidationResult($"Environment '{environment}' is not valid.");
            //     }
            // }

            return ValidationResult.Success;
        }
    }
}
