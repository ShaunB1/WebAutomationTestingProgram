using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public class AllowedBrowserTypeAttribute : ValidationAttribute
    {
        private readonly string[] _types;

        public AllowedBrowserTypeAttribute(string[] types)
        {
            _types = types;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string type)
            {
                if (!_types.Any(t => t.Equals(type, StringComparison.OrdinalIgnoreCase)))
                {
                    return new ValidationResult($"Browser Type '{type}' is not valid. Valid types: {string.Join(", ", _types)}");
                }
            }

            return ValidationResult.Success!;
        }
    }
}
