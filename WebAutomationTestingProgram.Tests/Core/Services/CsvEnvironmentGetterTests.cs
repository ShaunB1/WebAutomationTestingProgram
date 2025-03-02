using WebAutomationTestingProgram.Core.Services;
using WebAutomationTestingProgram.Core.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace WebAutomationTestingProgram.Tests.Core.Services;

public class CsvEnvironmentGetterTests
{
    private const string MockCsvData = @"
        EarlyON-AAD,,,,,,https://intra.dev2.edcs5.csc.gov.on.ca/LocationAdmin-AAD,https://intra.dev2.edcs5.csc.gov.on.ca/LocationAdmin-AAD,,,,,0,Oracle,EARLYON
        EarlyON-CI,,,,,,https://intra.dev2.edcs5.csc.gov.on.ca/LocationAdmin-CI,https://intra.dev2.edcs5.csc.gov.on.ca/LocationAdmin-CI,,,,,0,Oracle,EARLYON
        EDCS-1,cscgikdcdbora47.cihs.gov.on.ca,1521,EDCS1,OPS_WRITE,qateamrw1#,https://intra.dev.edcs7.csc.gov.on.ca/EDCS-1/Main/CSCITPortal.aspx,https://intra.dev2.edcs7.csc.gov.on.ca/EDCS-1/Main/CSCITPortal.aspx,\\cscgikdcapweb03\d\Files\EDCS-1\Emails,\\cscgikdcapweb03\d\Inetpub\wwwroot\EDCS-1,\\cscgikdcapweb01\d\inetpub\wwwroot\edcs-1,\\cscgikdcapweb03\d\Services\EDCS-1,0,ORACLE,EDCS
    ";

    private CsvEnvironmentGetter CreateCsvEnvironmentGetter()
    {
        var mockOptions = new Mock<IOptions<PathSettings>>();
        mockOptions.Setup(opt => opt.Value).Returns(new PathSettings { EnvironmentsListPath = "mock_environment_list.csv"});

        File.WriteAllText("mock_environment_list.csv", MockCsvData);
        
        return new CsvEnvironmentGetter(mockOptions.Object);
    }

    [Fact]
    public void Should_ReturnAllEnvironments()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        var environments = csvGetter.GetEnvironments();

        environments.Should().Contain(["EarlyON-AAD", "EarlyON-CI", "EDCS-1"]);
    }

    [Fact]
    public void Should_ThrowException_WhenFileIsMissing()
    {
        var mockOptions = new Mock<IOptions<PathSettings>>();
        mockOptions.Setup(opt => opt.Value).Returns(new PathSettings { EnvironmentsListPath = "nonexistent.csv"});
        
        var csvGetter = new CsvEnvironmentGetter(mockOptions.Object);
        
        Action act = () => csvGetter.GetEnvironments();
        act.Should().Throw<Exception>().WithMessage("environment_list.csv not found!");
    }

    [Fact]
    public void Should_ReturnCorrectEnvironment()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        csvGetter.GetEnvironmentName("EarlyON-AAD").Should().Be("EarlyON-AAD");
    }

    [Fact]
    public void Should_ReturnCorrectOpsBpsUrl()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        csvGetter.GetOpsBpsUrl("EarlyON-AAD").Should().Be("https://intra.dev2.edcs5.csc.gov.on.ca/LocationAdmin-AAD");
    }

    [Fact]
    public void Should_ThrowError_WhenEnvironmentIsMissing()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        var act = () => csvGetter.GetOpsBpsUrl("test");
        act.Should().Throw<Exception>().WithMessage("Failed to get environment name for 'test'");
    }

    [Fact]
    public void Should_ThrowException_WhenInputEmpty()
    {
        var mockFile = new Mock<IOptions<PathSettings>>();
        mockFile.Setup(opt => opt.Value).Returns(new PathSettings { EnvironmentsListPath = "environment_list.csv" });
        
        var csvGetter = new CsvEnvironmentGetter(mockFile.Object);
        Action act = () => csvGetter.GetOpsBpsUrl(" ");
        
        act.Should().Throw<Exception>().WithMessage("Environment cannot be empty.");
    }

    [Fact]
    public void Should_ReturnCorrectAadUrl()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        csvGetter.GetAadUrl("EarlyON-AAD").Should().Be("https://intra.dev2.edcs5.csc.gov.on.ca/LocationAdmin-AAD");
    }

    [Fact]
    public void Should_ReturnCorrectHost()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        csvGetter.GetHost("EDCS-1").Should().Be("cscgikdcdbora47.cihs.gov.on.ca");
    }

    [Fact]
    public void Should_ReturnCorrectPort()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        csvGetter.GetPort("EDCS-1").Should().Be("1521");
    }

    [Fact]
    public void Should_ReturnEncryptionStatus()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        csvGetter.GetIsEncrypted("EarlyON-AAD").Should().Be("0");
    }

    [Fact]
    public void Should_ReturnUserName()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        csvGetter.GetUsername("EDCS-1").Should().Be("OPS_WRITE");
    }

    [Fact]
    public void Should_ReturnPassword()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        csvGetter.GetPassword("EDCS-1").Should().Be("qateamrw1#");
    }

    [Fact]
    public void Should_ReturnDbName()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        csvGetter.GetDbName("EDCS-1").Should().Be("EDCS1");
    }

    [Fact]
    public void Should_ReturnAppType()
    {
        var csvGetter = CreateCsvEnvironmentGetter();
        csvGetter.GetApplicationType("EDCS-1").Should().Be("EDCS");
    }
}