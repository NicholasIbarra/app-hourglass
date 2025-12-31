using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;

namespace IntegrationSolution.ServiceDefaults.Controllers;

public static partial class ControllerExtensions
{
    public sealed class EnforceProblemDetailsFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult &&
                objectResult.StatusCode is >= 400 &&
                objectResult.Value is string message)
            {
                context.Result = new ObjectResult(new ProblemDetails
                {
                    Status = objectResult.StatusCode,
                    Title = ReasonPhrases.GetReasonPhrase(objectResult.StatusCode.Value),
                    Detail = message
                })
                {
                    StatusCode = objectResult.StatusCode
                };
            }

            await next();
        }
    }

}
