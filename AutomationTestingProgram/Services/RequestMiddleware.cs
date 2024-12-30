namespace AutomationTestingProgram.Services
{
    /// <summary>
    /// Middleware used to limit total # of active requests
    /// </summary>
    public class RequestMiddleware
    {
        private static readonly SemaphoreSlim _maxRequests = new SemaphoreSlim(10);

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
            if (!_maxRequests.Wait(0))
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
                _maxRequests.Release();
            }
        }

        public static async Task ReadyForTermination()
        {
            while (_maxRequests.CurrentCount != 10)
            {
                await Task.Delay(100);
            }
        }


    }
}
