using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Oekolariet.Application.Common.Errors;

namespace Oekolariet.Api.ErrorHandling;

internal sealed class ProblemDetailsExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            // Map Exceptions to HTTP Status Codes
            NotFoundException => (StatusCodes.Status404NotFound, "Ikke fundet"),
            ConflictException => (StatusCodes.Status409Conflict, "Konflikt"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Ugyldig forespørgsel"),
            _ => (0, string.Empty),
        };

        if (status == 0)
        {
            return false; // On unknown exception; let the default handler give a 500
        }

        context.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
                Instance = context.Request.Path,
            },
        });
    }
}
