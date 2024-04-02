namespace API.Middleware
{
    public class LoggingMiddleware : IMiddleware
    {
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var queryString = context.Request.QueryString.ToString();
            //紀錄request
            _logger.LogInformation($"Request：【Method】{context.Request.Method}【Path】{context.Request.Path}【Query String】{queryString}");
 
            if (context.Request.Body.CanSeek)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                if (!string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogInformation($"Request Body：{requestBody}");
                }
            }

            await next(context);

            _logger.LogInformation($"Response：{context.Response.StatusCode}");


            if (context.Response.Body.CanSeek)
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                string responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                if (!string.IsNullOrEmpty(responseBody))
                {
                    _logger.LogInformation($"Response Body：{responseBody}、{queryString}");
                }
            }
        }
    }
}
