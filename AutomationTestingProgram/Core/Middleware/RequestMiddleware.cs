using AutomationTestingProgram.Core.Services;

namespace AutomationTestingProgram.Core.Middleware
{
    /// <summary>
    /// Middleware used to limit total # of active requests
    /// </summary>
    public class RequestMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestMiddleware> _logger;
        private readonly RequestHandler _handler;
        private static readonly List<string> _rateLimitedControllers = ["Test", "Environments", "Core"];

        // Constructor to inject middleware
        public RequestMiddleware(RequestDelegate next, ILogger<RequestMiddleware> logger, RequestHandler handler)
        {
            _next = next;
            _logger = logger;
            _handler = handler;
        }

        // Handles request limiting
        public async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var controller = routeData?.Values["controller"]?.ToString();
            var isRateLimited = !string.IsNullOrEmpty(controller) && _rateLimitedControllers.Contains(controller);
            
            if (isRateLimited)
            {
                if (!await _handler.TryAcquireSlotAsync(0)) // 5 min timeout?
                {
                    context.Response.StatusCode = 503;
                    _logger.LogError($"Server Busy: Too many requests. Please try again later.");
                    await context.Response.WriteAsync("Server Busy: Too many requests. Please try again later");
                    return;
                }
            }

            try
            {
                await _next(context);
            }
            finally
            {
                if (isRateLimited)
                {
                    _handler.ReleaseSlot();
                }
            }
        }
    }
}
