using System.Security.Claims;
using Arc4u.Dependency.Attribute;

namespace Arc4u.Security.Principal;

[Export(typeof(IApplicationContext)), Shared]

public class ApplicationClaimsPrincipalSelectorContext : IApplicationContext
{
    public AppPrincipal? Principal => ClaimsPrincipal.Current as AppPrincipal;

    /// <summary>
    /// Gets or sets the activity ID.
    /// </summary>
    /// <value>The activity ID.</value>
    public string ActivityID { get; set; } = string.Empty;

    public void SetPrincipal(AppPrincipal principal)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(principal);
#else
        if (null == principal)
        {
            throw new ArgumentNullException(nameof(principal));
        }
#endif
        Thread.CurrentPrincipal = principal;
    }
}
