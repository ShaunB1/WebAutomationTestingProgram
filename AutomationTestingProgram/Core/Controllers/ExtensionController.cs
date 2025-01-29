using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Core;

[ApiController]
[Route("api/[controller]")]
public class ExtensionController : CoreController
{
    private readonly string _extensionDownloadPath;
    public ExtensionController(ICustomLoggerProvider provider, IOptions<PathSettings> options)
        :base(provider)
    {
        _extensionDownloadPath = options.Value.ExtensionDownloadPath;
    }

    [Authorize]
    [HttpGet("download-zip")]
    public IActionResult DownloadZip()
    {
        try
        {
            if (System.IO.File.Exists(_extensionDownloadPath))
            {
                return PhysicalFile(_extensionDownloadPath, "application/octet-stream", "TAP_Extension.zip");
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }
}