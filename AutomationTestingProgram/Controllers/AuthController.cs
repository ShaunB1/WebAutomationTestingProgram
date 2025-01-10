using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    public AuthController()
    {
        
    }

    [Authorize]
    [HttpGet("validateToken")]
    public async Task<IActionResult> ValidateToken() 
    {
        var user = HttpContext.User;
        if (user == null)
        {
            return Unauthorized("No user found in the token");
        }

        return Ok(new
        {
            message = "Token is valid"
        });
    }

    [Authorize]
    [HttpGet("getAccountInfo")]
    public async Task<IActionResult> GetAccountInfo()
    {
        var user = HttpContext.User;
        if (user == null)
        {
            return Unauthorized("No user found in the token");
        }

        var name = user.FindFirst("name")?.Value;
        var email = user.FindFirst("preferred_username")?.Value;

        return Ok(new
        {
            message = "Token is valid",
            name,
            email
        });
    }
}