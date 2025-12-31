using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServiceDefaults.ExceptionHandling;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Status = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                ValidationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            },
            Title = "An error occurred",
            Type = exception.GetType().Name,
            Detail = exception.Message
        };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}


//public class GlobalExceptionHandler : IExceptionHandler
//{
//    private readonly ILogger<GlobalExceptionHandler> _logger;

//    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
//    {
//        _logger = logger;
//    }

//    public async ValueTask<bool> TryHandleAsync(
//        HttpContext httpContext,
//        Exception exception,
//        CancellationToken cancellationToken)
//    {
//        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

//        var problemDetails = exception switch
//        {
//            ValidationException validationException => CreateValidationProblemDetails(validationException),
//            ArgumentException or ArgumentNullException => new ProblemDetails
//            {
//                Status = StatusCodes.Status400BadRequest,
//                Title = "Bad Request",
//                Detail = exception.Message,
//                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
//            },
//            _ => new ProblemDetails
//            {
//                Status = StatusCodes.Status500InternalServerError,
//                Title = "Internal Server Error",
//                Detail = "An unexpected error occurred. Please try again later.",
//                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
//            }
//        };

//        problemDetails.Instance = httpContext.Request.Path;
//        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

//        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
//        httpContext.Response.ContentType = "application/problem+json";

//        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

//        return true;
//    }

//    private static ProblemDetails CreateValidationProblemDetails(ValidationException validationException)
//    {
//        var errors = validationException.Errors
//            .GroupBy(e => e.PropertyName)
//            .ToDictionary(
//                g => g.Key,
//                g => g.Select(e => e.ErrorMessage).ToArray()
//            );

//        return new ValidationProblemDetails(errors)
//        {
//            Status = StatusCodes.Status400BadRequest,
//            Title = string.Join(",", errors.First().Value),
//            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
//        };
//    }
//}
