using System.Diagnostics;
using AutomationTestingProgram.Core;
using Microsoft.AspNetCore.Mvc;

namespace AutomationTestingProgram.Controllers; // Dont change namespace

public class HomeController : Controller
{
    private readonly ICustomLogger _logger;

    public HomeController(ICustomLoggerProvider provider)
    {
        _logger = provider.CreateLogger<HomeController>();
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}