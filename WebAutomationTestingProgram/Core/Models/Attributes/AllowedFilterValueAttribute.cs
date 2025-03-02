using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using WebAutomationTestingProgram.Core.Helpers.Requests;
using WebAutomationTestingProgram.Core.Requests;

namespace WebAutomationTestingProgram.Core.Models.Attributes
{
    public class AllowedFilterValueAttribute : ValidationAttribute
    {
        private const string IdPattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
        private readonly string _filterTypeProperty;

        public AllowedFilterValueAttribute(string filterTypeProperty)
        {
            _filterTypeProperty = filterTypeProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string val) return new ValidationResult("invalid input. Input must be of type string.");
            
            var instance = validationContext.ObjectInstance;

            var filterTypeProperty = instance.GetType().GetProperty(_filterTypeProperty);
            if (filterTypeProperty == null)
            {
                return new ValidationResult($"Property {_filterTypeProperty} not found.");
            }

            var filterTypeAsString = filterTypeProperty.GetValue(instance) as string;
            if (string.IsNullOrWhiteSpace(filterTypeAsString))
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
                case FilterType.Id:
                    if (!Regex.IsMatch(val, IdPattern))
                    {
                        return new ValidationResult($"ID must be in correct format");
                    }
                    break;
                case FilterType.Type:
                    var valueType = Type.GetType(val);
                    
                    if (valueType == null)
                    {
                        return new ValidationResult($"Type Value Invalid - Not Found");
                    }

                    if (!valueType.IsClass || !typeof(IClientRequest).IsAssignableFrom(valueType)) {
                        return new ValidationResult($"Type Value Invalid -- Not Proper Type");
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return ValidationResult.Success;
        }
    }
}
