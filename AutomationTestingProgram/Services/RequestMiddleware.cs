using AutomationTestingProgram.Backend;

namespace AutomationTestingProgram.Services
{
    /// <summary>
    /// Middleware used to limit total # of active requests
    /// </summary>
    public class RequestMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestMiddleware> _logger;

        // Constructor to inject middleware
        public RequestMiddleware(RequestDelegate next, ILogger<RequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Handles request limiting
        public async Task InvokeAsync(HttpContext context)
        {
            if (!RequestHandler.TryAcquireRequestSlot())
            {
                context.Response.StatusCode = 503;
                _logger.LogError($"Too many requests. Please try again later.");
                await context.Response.WriteAsync("Too many requests. Please try again later");
                return;
            }

            try
            {
                await _next(context);
            }
            finally
            {
                RequestHandler.ReleaseRequestSlot();
            }
        }
    }
}
