using System.ComponentModel.DataAnnotations;
using AutomationTestingProgram.Modules.TestRunner.Models.Attributes;
using FluentAssertions;

namespace AutomationTestingProgram.Tests.Modules.TestRunner.Models.Attributes;

public class ValidBrowserAttributeTests
{
    [Fact]
    public void Should_ReturnSuccess_When_BrowserIsSupported()
    {
        var attribute = new ValidBrowserAttribute(["Chrome", "Edge", "Firefox"]);
        var browser = "Chrome";
        
        var result = attribute.GetValidationResult(browser, new ValidationContext(browser));
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void Should_ReturnSuccess_When_BrowserIsUppercased()
    {
        var attribute = new ValidBrowserAttribute(["Chrome", "Edge", "Firefox"]);
        var browser = "edge";
        
        var result = attribute.GetValidationResult(browser, new ValidationContext(browser));
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void Should_ReturnError_When_BrowserIsNotSupported()
    {
        var attribute = new ValidBrowserAttribute(["Chrome", "Edge", "Firefox"]);
        var browser = "Opera";

        var result = attribute.GetValidationResult(browser, new ValidationContext(browser));
        result.ErrorMessage.Should().Be($"Browser {browser} is not supported. Allowed browsers: chrome, edge, firefox");
    }

    [Fact]
    public void Should_ReturnError_When_BrowserIsEmpty()
    {
        var attribute = new ValidBrowserAttribute(["Chrome", "Edge", "Firefox"]);
        var browser = "";

        var result = attribute.GetValidationResult(browser, new ValidationContext(browser));
        result.ErrorMessage.Should().Be("Browser cannot be empty. Allowed browsers: chrome, edge, firefox");
    }

    [Fact]
    public void Should_ReturnError_When_BrowserIsNull()
    {
        var attribute = new ValidBrowserAttribute(["Chrome", "Edge", "Firefox"]);
        object? browser = null;

        var result = attribute.GetValidationResult(browser, new ValidationContext(new object()));
        result.ErrorMessage.Should().Be("Invalid input. Input must be a string.");
    }
}