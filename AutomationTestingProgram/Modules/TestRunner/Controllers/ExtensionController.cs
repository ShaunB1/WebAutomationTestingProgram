using AutomationTestingProgram.Actions;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.HPSF;
using Sprache;
using System.IO.Compression;
using System.Web.Http.Results;

[ApiController]
[Route("api/[controller]")]
public class ExtensionController : ControllerBase
{
    private readonly string _extensionDownloadPath;
    public ExtensionController()
    {
        _extensionDownloadPath = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build()["ExtensionDownloadPath"];
    }

    [Authorize]
    [HttpGet("download-zip")]
    public async Task<IActionResult> DownloadZip()
    {
        if (System.IO.File.Exists(_extensionDownloadPath))
        {
            return PhysicalFile(_extensionDownloadPath, "application/octet-stream", "TAP_Extension.zip");
        }
        else
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}