using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebAutomationTestingProgram.Core.Settings;

namespace WebAutomationTestingProgram.Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExtensionController : ControllerBase
{
    private readonly string _extensionDownloadPath;
    public ExtensionController(IOptions<PathSettings> options)
    {
        _extensionDownloadPath = options.Value.ExtensionDownloadPath;
    }

    /*
     * curl -X GET -H "Content-Type: application/json" https://localhost:7117/api/extension/download-zip
     */

    [Authorize]
    [HttpGet("download-zip")]
    [ResponseCache(Duration = 14400, Location = ResponseCacheLocation.Client)] // Cached for four hours
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
            });
        }
    }
}