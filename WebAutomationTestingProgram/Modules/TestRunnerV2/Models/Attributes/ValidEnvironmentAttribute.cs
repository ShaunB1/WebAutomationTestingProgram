﻿using System.ComponentModel.DataAnnotations;
using WebAutomationTestingProgram.Core.Services;

namespace WebAutomationTestingProgram.Modules.TestRunnerV2.Models.Attributes
{
    public class ValidEnvironmentAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string environment)
            {
                return new ValidationResult("Invalid input. Input must be a string.");
            }

            var environmentGetter = validationContext.GetService(typeof(CsvEnvironmentGetter)) as CsvEnvironmentGetter;

            try
            {
                var envName = environmentGetter?.GetEnvironmentName(environment);
            }
            catch
            {
                return new ValidationResult($"Environment '{environment}' is not supported.");
            }

            return ValidationResult.Success;
        }
    }
}
