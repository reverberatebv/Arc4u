using System.Diagnostics;
using System.Globalization;
using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Arc4u.OAuth2.Middleware;
public class AddContextToPrincipalMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ActivitySource? _activitySource;

    public AddContextToPrincipalMiddleware(RequestDelegate next, IActivitySourceFactory activitySourceFactory)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _activitySource = activitySourceFactory.GetArc4u();
    }

    public async Task InvokeAsync(HttpContext context, ILogger<AddContextToPrincipalMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.User is not null && context.User is AppPrincipal principal && principal.Identity is not null && principal.Identity.IsAuthenticated)
        {
            using var activity = _activitySource?.StartActivity("Add context to Arc4u Principal", ActivityKind.Producer);

            // Ensure we have a traceparent.
            if (!context.Request.Headers.ContainsKey("traceparent"))
            {
                // Activity.Current is not null just because we are in the context of an activity.
                context.Request.Headers.Append("traceparent", Activity.Current!.Id);
            }

            activity?.SetTag(LoggingConstants.ActivityId, Activity.Current!.Id);

            // Check for a culture.
            var cultureHeader = context.Request?.Headers?.FirstOrDefault(h => h.Key.Equals("culture", StringComparison.InvariantCultureIgnoreCase));
            if (cultureHeader.HasValue && StringValues.Empty != cultureHeader.Value.Value && cultureHeader.Value.Value.Any())
            {
                try
                {
                    principal.Profile.CurrentCulture = new CultureInfo(cultureHeader.Value.Value[0]!);
                }
                catch (Exception ex)
                {
                    logger.Technical().LogException(ex);
                }
            }
        }

        await _next(context).ConfigureAwait(false);
    }

}
