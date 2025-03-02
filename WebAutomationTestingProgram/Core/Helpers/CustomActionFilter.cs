using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace WebAutomationTestingProgram.Core.Helpers;

public class CustomActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        IHeaderDictionary headers = context.HttpContext.Response.Headers;

        // Cache-Control header
        if (!headers.ContainsKey("Cache-Control"))
        {
            headers.Add("Cache-Control", "no-cache");
        }
        else
        {
            headers["Cache-Control"] = new StringValues("no-cache");
        }

        // Connection header
        if (!headers.ContainsKey("Connection"))
        {
            headers.Add("Connection", "keep-alive");
        }
        else
        {
            headers["Connection"] = new StringValues("keep-alive");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Currently, no action is taken after the action method executes
    }
}