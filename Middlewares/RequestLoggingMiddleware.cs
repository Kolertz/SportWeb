namespace SportWeb.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = Guid.NewGuid().ToString();
            using (_logger.BeginScope("RequestId: {RequestId}", requestId))
            {
                _logger.LogInformation($"========== Start of Request {requestId}; {DateTime.UtcNow:HH:mm:ss.fff}=============================================");

                await _next(context);
                _logger.LogInformation($"========== End of Request {requestId}; {DateTime.UtcNow:HH:mm:ss.fff}===============================================");
            }
        }
    }
}
