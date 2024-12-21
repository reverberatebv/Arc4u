using Microsoft.AspNetCore.Http;

namespace Arc4u.AspNetCore.Middleware;

public class GrpcAuthenticationStatusCodeMiddleware
{
    private readonly RequestDelegate _next;

    public GrpcAuthenticationStatusCodeMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next.Invoke(context).ConfigureAwait(false);

        if (null != context.Request.ContentType &&
            null != context.Response &&
            context.Request.ContentType.Contains("grpc") &&
            context.Response.StatusCode == 302)
        {
            context.Response.Headers.Clear();
            context.Response.StatusCode = 401;
        }
    }
}
