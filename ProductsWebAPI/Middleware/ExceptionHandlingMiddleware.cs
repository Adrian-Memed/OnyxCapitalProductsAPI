using System.Net;

namespace ProductsWebAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                _logger.LogInformation("A request has come in.");

                await _next(context);

                _logger.LogInformation("A request has completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            if (exception is FluentValidation.ValidationException validationException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                var validationErrors = validationException.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                });

                await context.Response.WriteAsJsonAsync(new
                {
                    Title = "Validation Error",
                    Status = context.Response.StatusCode,
                    Errors = validationErrors
                });
                return;
            }
                                   
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                Title = "Internal Server Error",
                Status = context.Response.StatusCode,
                Errors = exception.Message
            });
            return;
        }
    }
}
