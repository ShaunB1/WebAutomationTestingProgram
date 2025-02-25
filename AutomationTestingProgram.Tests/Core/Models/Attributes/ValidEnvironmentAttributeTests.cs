using System.ComponentModel.DataAnnotations;
using AutomationTestingProgram.Modules.TestRunner.Models.Attributes;
using FluentAssertions;

namespace AutomationTestingProgram.Tests.Core.Models.Attributes;

public class ValidEnvironmentAttributeTests
{
    [Fact]
    public void Should_ReturnSuccess_When_EnvironmentIsValid()
    {
        var attribute = new ValidEnvironmentAttribute();
        const string environment = "EDCS-1";

        var result = attribute.GetValidationResult(environment, new ValidationContext(environment));
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void Should_ReturnError_When_EnvironmentIsNotValid()
    {
        var attribute = new ValidEnvironmentAttribute();
        const string environment = "EDCS-INVALID";
        
        var result = attribute.GetValidationResult(environment, new ValidationContext(environment));
        result?.ErrorMessage.Should().Be($"Environment '{environment}' is not supported.");
    }

    [Fact]
    public void Should_ReturnError_When_InputIsNotString()
    {
        var attribute = new ValidEnvironmentAttribute();
        const int environment = 1;
        
        var result = attribute.GetValidationResult(environment, new ValidationContext(environment));
        result?.ErrorMessage.Should().Be("Invalid input. Input must be a string.");
    }
}