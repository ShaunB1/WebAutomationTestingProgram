using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Core.Models.Attributes;
public class ValidFileAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public ValidFileAttribute(string[] extensions)
    {
        _extensions = extensions
            .Select(ext => ext.StartsWith('.') ? ext : "." + ext)
            .ToArray();
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IFormFile file)
        {
            return new ValidationResult("Invalid input. Input must be of type IFormFile.");
        }
        
        if (string.IsNullOrWhiteSpace(file.FileName))
        {
            return new ValidationResult("Invalid file name.");
        }
        
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!_extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return new ValidationResult($"Invalid file type. Allowed file types: {string.Join(", ", _extensions)}.");
        }

        return ValidationResult.Success;
    }
}
