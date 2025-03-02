using System.ComponentModel.DataAnnotations;

namespace WebAutomationTestingProgram.Modules.TestRunner.Models.Attributes
{
    public class ValidBrowserAttribute : ValidationAttribute
    {
        private readonly string[] _browsers;

        public ValidBrowserAttribute(string[] browsers)
        {
            _browsers = browsers
                .Select(b => b.ToLowerInvariant())
                .ToArray();
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string browser)
            {
                return new ValidationResult("Invalid input. Input must be a string.");
            }

            if (string.IsNullOrWhiteSpace(browser))
            {
                return new ValidationResult($"Browser cannot be empty. Allowed browsers: {string.Join(", ", _browsers)}");
            }

            if (!_browsers.Contains(browser, StringComparer.OrdinalIgnoreCase))
            {
                return new ValidationResult($"Browser {browser} is not supported. Allowed browsers: {string.Join(", ", _browsers)}");
            }

            return ValidationResult.Success;
        }
    }
}
