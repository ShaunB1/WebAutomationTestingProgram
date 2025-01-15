using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AutomationTestingProgram.Core
{
    public class AllowedFilterValueAttribute : ValidationAttribute
    {
        private static readonly string IDPattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";

        private readonly string _filterTypeProperty;

        public AllowedFilterValueAttribute(string filterTypeProperty)
        {
            _filterTypeProperty = filterTypeProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string val)
            {
                var instance = validationContext.ObjectInstance;

                var filterTypeProperty = instance.GetType().GetProperty(_filterTypeProperty);
                if (filterTypeProperty == null)
                {
                    return new ValidationResult($"Property {_filterTypeProperty} not found.");
                }

                var filterTypeAsString = filterTypeProperty.GetValue(instance) as string;
                if (string.IsNullOrEmpty(filterTypeAsString))
                {
                    return new ValidationResult($"FilterType is required");
                }

                if (!Enum.TryParse(filterTypeAsString, out FilterType filterType))
                {
                    return new ValidationResult($"Filter Type {filterTypeAsString} is not valid.");
                }

                switch (filterType)
                {
                    case FilterType.None:
                        break;
                    case FilterType.ID:
                        if (!Regex.IsMatch(val, IDPattern))
                        {
                            return new ValidationResult($"ID must be in correct format");
                        }
                        break;
                    case FilterType.Type:
                        Type? valueType = Type.GetType(val);
                        if (valueType == null)
                        {
                            return new ValidationResult($"Type Value Invalid - Not Found");
                        }

                        if (!valueType.IsClass || !typeof(IClientRequest).IsAssignableFrom(valueType)) {
                            return new ValidationResult($"Type Value Invalid -- Not Proper Type");
                        }
                        break;
                }
            }

            return ValidationResult.Success;
        }
    }
}
