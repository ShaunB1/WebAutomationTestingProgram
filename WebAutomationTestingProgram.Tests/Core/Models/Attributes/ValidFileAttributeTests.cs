using System.ComponentModel.DataAnnotations;
using WebAutomationTestingProgram.Core.Models.Attributes;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace WebAutomationTestingProgram.Tests.Core.Models.Attributes;

public class ValidFileAttributeTests
{
    [Fact]
    public void Should_ReturnSuccess_When_FileExtensionIsAllowed()
    {
        var attribute = new ValidFileAttribute([".xls", ".xlsx", ".xlsm"]);

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.xlsx");
        
        var result = attribute.GetValidationResult(mockFile.Object, new ValidationContext(mockFile.Object));

        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void Should_ReturnError_When_FileExtensionIsNotAllowed()
    {
        var attribute = new ValidFileAttribute([".xls", ".xlsx", ".xlsm"]);
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.jpeg");
        
        var result = attribute.GetValidationResult(mockFile.Object, new ValidationContext(mockFile.Object));
        
        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Be("Invalid file type. Allowed file types: .xls, .xlsx, .xlsm.");
    }

    [Fact]
    public void Should_ReturnError_When_NotIFormFile()
    {
        var attribute = new ValidFileAttribute([".xls", ".xlsx", ".xlsm"]);

        var invalidInput = "this is not a file";
        
        var result = attribute.GetValidationResult(invalidInput, new ValidationContext(invalidInput));
        
        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Be("Invalid input. Input must be of type IFormFile.");
    }

    [Fact]
    public void Should_ReturnError_When_FileIsNull()
    {
        var attribute = new ValidFileAttribute([".xls", ".xlsx", ".xlsm"]);

        var validationContext = new ValidationContext(new object());
        
        var result = attribute.GetValidationResult(null, new ValidationContext(validationContext));
        
        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Be("Invalid input. Input must be of type IFormFile.");
    }

    [Fact]
    public void Should_ReturnError_When_FileNameIsEmpty()
    {
        var attribute = new ValidFileAttribute([".xls", ".xlsx", ".xlsm"]);
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(string.Empty);

        var result = attribute.GetValidationResult(mockFile.Object, new ValidationContext(mockFile.Object));
        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Be("Invalid file name.");
    }

    [Fact]
    public void Should_ReturnSuccess_When_FileExtensionIsUppercase()
    {
        var attribute = new ValidFileAttribute([".xls", ".xlsx", ".xlsm"]);
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("TEST.XLS");
        
        var result = attribute.GetValidationResult(mockFile.Object, new ValidationContext(mockFile.Object));
        
        result.Should().Be(ValidationResult.Success);
    }

    [Fact] 
    public void Should_ReturnSuccess_When_FileExtensionWithoutLeadingDot()
    {
        var attribute = new ValidFileAttribute(["xls", "xlsx", "xlsm"]);
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.xlsx");

        var result = attribute.GetValidationResult(mockFile.Object, new ValidationContext(mockFile.Object));
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void Should_ReturnSuccess_When_FileHasMultipleDots()
    {
        var attribute = new ValidFileAttribute([".xls", ".xlsx", ".xlsm"]);
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.dot.xlsx");
        
        var result = attribute.GetValidationResult(mockFile.Object, new ValidationContext(mockFile.Object));
        result.Should().Be(ValidationResult.Success);
    }
}