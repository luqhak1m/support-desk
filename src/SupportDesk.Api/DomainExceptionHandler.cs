using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Domain;

namespace SupportDesk.Api;

// Turns exceptions into RFC 7807 ProblemDetails so a client never sees a stack trace.
// A broken business rule is a 409 Conflict carrying the reason it was rejected.
public class DomainExceptionHandler:IExceptionHandler
{
    private readonly ILogger<DomainExceptionHandler> _logger;

    public DomainExceptionHandler(ILogger<DomainExceptionHandler> logger)
    {
        _logger=logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problem=new ProblemDetails();

        if(exception is DomainRuleViolationException)
        {
            problem.Status=StatusCodes.Status409Conflict;
            problem.Title="Business rule violation";
            problem.Detail=exception.Message;
            problem.Type="https://tools.ietf.org/html/rfc7231#section-6.5.8";

            _logger.LogInformation("Rule rejected: {Message}",exception.Message);
        }
        else
        {
            problem.Status=StatusCodes.Status500InternalServerError;
            problem.Title="An unexpected error occurred";
            problem.Detail="Please try again, or contact support if the problem continues.";

            _logger.LogError(exception,"Unhandled exception");
        }

        httpContext.Response.StatusCode=problem.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problem,cancellationToken);

        return true;
    }
}
