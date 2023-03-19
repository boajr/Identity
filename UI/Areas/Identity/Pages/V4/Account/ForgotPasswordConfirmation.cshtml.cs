using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Boa.Identity.UI.V4.Pages.Account.Internal;

/// <summary>
///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
///     directly from your code. This API may change or be removed in future releases.
/// </summary>
[AllowAnonymous]
public class BoaForgotPasswordConfirmation : PageModel
{
    private readonly IEnumerable<IResetPasswordService> _resetPasswordServices;

    /// <summary>
    ///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string Message { get; private set; } = "Unknown method";

    /// <summary>
    ///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public BoaForgotPasswordConfirmation(IEnumerable<IResetPasswordService> resetPasswordServices)
    {
        _resetPasswordServices = resetPasswordServices;
    }

    /// <summary>
    ///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public void OnGet()
    {
        IResetPasswordService? service = _resetPasswordServices.FirstOrDefault(s => s.ServiceName == Request.Query["method"]);
        if (service != null)
        {
            Message = service.ConfirmationMessage;
        }
    }
}
