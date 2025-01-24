namespace AutomationTestingProgram.Core
{
    /// <summary>
    /// Middleware used to limit total # of active requests
    /// </summary>
    public class RequestMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestMiddleware> _logger;

        private static readonly List<string> RateLimitedControllers = new List<String>
        {
            "Test",
            "Environments",
            "Core",
            "Extension",
        };

        // Constructor to inject middleware
        public RequestMiddleware(RequestDelegate next, ILogger<RequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Handles request limiting
        public async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var controller = routeData?.Values["controller"]?.ToString();

            bool isRateLimited = !string.IsNullOrEmpty(controller) && RateLimitedControllers.Contains(controller);


            if (isRateLimited)
            {
                if (!await RequestHandler.TryAcquireSlotAsync(0)) // 5 min timeout?
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
                    RequestHandler.ReleaseSlot();
                }
            }
        }
    }
}
