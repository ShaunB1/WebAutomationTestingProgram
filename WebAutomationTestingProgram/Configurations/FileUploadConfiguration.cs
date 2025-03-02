using Microsoft.AspNetCore.Http.Features;

namespace WebAutomationTestingProgram.Configurations;

public class FileUploadConfiguration
{
    private const long BodyLengthLimit = 15 * 1024 * 1024;
    private const int ValueLengthLimit = 10 * 1024 * 1024;
    private const int HeadersCountLimit = 100;
    
    public static void Configure(WebApplicationBuilder builder)
    {
        ConfigureFileUpload(builder);
    }

    private static void ConfigureFileUpload(WebApplicationBuilder builder)
    {
        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = BodyLengthLimit;
            options.ValueLengthLimit = ValueLengthLimit;
            options.MultipartHeadersCountLimit = HeadersCountLimit;
        });
    }
}