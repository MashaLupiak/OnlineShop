using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnlineShop.API.Exceptions;


namespace OnlineShop.API.Middleware
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
                await _next(context); 
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"An error occurred while processing request: {context.Request.Path}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode;
            string message;

            switch (exception)
            {
                case ArgumentException:
                    statusCode = 400;
                    message = "Invalid request parameters.";
                    break;
                case NotFoundException ex:
                    statusCode = 404;
                    message = "Resource not found.";
                    break;
                case UnauthorizedAccessException:
                    statusCode = 401;
                    message = "Unauthorized access.";
                    break;
                default:
                    statusCode = 500;
                    message = "Internal server error.";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new { message });
        }
    }
}
