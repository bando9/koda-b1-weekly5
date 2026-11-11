
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Mvc.Middlewares;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
    {
        _logger=logger;
        _problemDetailsService=problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if(exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        _logger.LogError(exception, "Unhandled request error. Method: {Method}, Path: {Path}, TraceId: {TraceId}", httpContext.Request.Method, httpContext.Request.Path,httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Status= StatusCodes.Status500InternalServerError,
            Title= "Internal Server Error",
            Detail= "Ada masalah server abangkuhhh",
            Instance= httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        var written = await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext=httpContext,
            ProblemDetails=problem,
            Exception=exception
        });

        if (!written)
        {
            httpContext.Response.ContentType = "text/plain; charset=utf-8";

            await httpContext.Response.WriteAsync("Ga expect, servernya Error", cancellationToken);
        }

        return true;
    }

}