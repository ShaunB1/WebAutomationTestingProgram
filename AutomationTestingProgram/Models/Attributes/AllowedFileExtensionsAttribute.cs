﻿using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Models;
public class AllowedFileExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedFileExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_extensions.Contains(extension))
            {
                return new ValidationResult($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", _extensions)}");
            }
        }

        return ValidationResult.Success!;
    }
}
