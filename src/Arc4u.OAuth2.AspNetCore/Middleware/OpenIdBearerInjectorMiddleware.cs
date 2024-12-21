using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Middleware;

public class OpenIdBearerInjectorMiddleware
{
    private readonly OpenIdBearerInjectorSettingsOptions _options;
    private readonly RequestDelegate _next;
    private ActivitySource? _activitySource;

    public OpenIdBearerInjectorMiddleware([DisallowNull] RequestDelegate next, [DisallowNull] OpenIdBearerInjectorSettingsOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync([DisallowNull] HttpContext context, IActivitySourceFactory activitySourceFactory, ILogger<OpenIdBearerInjectorMiddleware> logger)
    {
        if (context.User is not null && context.User.Identity is not null && context.User.Identity.IsAuthenticated && context.User.Identity.AuthenticationType!.Equals(_options.OpenIdSettings.Values[TokenKeys.AuthenticationTypeKey], StringComparison.InvariantCultureIgnoreCase))
        {
            if (context.User is AppPrincipal principal)
            {
                if (null != principal?.Profile?.CurrentCulture)
                {
                    context.Request?.Headers?.Append("culture", principal.Profile.CurrentCulture.TwoLetterISOLanguageName);
                }
            }

            _activitySource ??= activitySourceFactory.GetArc4u();

            using var activity = _activitySource?.StartActivity("Inject bearer token in header", ActivityKind.Producer);
            TokenInfo? tokenInfo = null;

            if (_options.OnBehalfOfOpenIdSettings is not null && _options.OnBehalfOfOpenIdSettings.Values.Any())
            {
                try
                {
                    var provider = context.RequestServices.GetKeyedService<ITokenProvider>(_options.OboProviderKey);

                    if (provider is null)
                    {
                        logger.Technical().LogError($"The token provider {_options.OboProviderKey} is not found!");
                        return;
                    }

                    tokenInfo = await provider.GetTokenAsync(_options.OnBehalfOfOpenIdSettings, null).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.Technical().Exception(ex).Log();
                }

            }
            else
            {
                try
                {
                    var provider = context.RequestServices.GetKeyedService<ITokenProvider>(_options.OpenIdSettings.Values[TokenKeys.ProviderIdKey]);

                    if (provider is null)
                    {
                        logger.Technical().LogError($"The token provider {_options.OpenIdSettings.Values[TokenKeys.ProviderIdKey]} is not found!");
                        return;
                    }

                    tokenInfo = await provider.GetTokenAsync(_options.OpenIdSettings, null).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.Technical().Exception(ex).Log();
                }
            }

            if (tokenInfo is not null)
            {
                var authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.Token).ToString();
                context.Request!.Headers.Remove("Authorization");
                context.Request.Headers.Append("Authorization", authorization);
            }
        }

        await _next.Invoke(context).ConfigureAwait(false);
    }
}

