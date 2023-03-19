using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;

namespace Boa.Identity.UI.V5.Pages.Account.Internal;

/// <summary>
///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
///     directly from your code. This API may change or be removed in future releases.
/// </summary>
[AllowAnonymous]
[BoaIdentityDefaultUI(typeof(BoaForgotPasswordModel<>))]
public abstract class BoaForgotPasswordModel : PageModel
{
    /// <summary>
    ///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    /// <summary>
    ///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        public string Method { get; set; } = default!;
    }

    /// <summary>
    ///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string Message { get; protected set; } = default!;

    /// <summary>
    ///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public object? Data { get; protected set; }

    /// <summary>
    ///     This API supports the Boa Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public virtual Task<IActionResult> OnPostAsync() => throw new NotImplementedException();
}

internal sealed class BoaForgotPasswordModel<TUser> : BoaForgotPasswordModel where TUser : class
{
    private readonly IEnumerable<IResetPasswordService> _resetPasswordServices;

    public BoaForgotPasswordModel(IEnumerable<IResetPasswordService> resetPasswordServices)
    {
        _resetPasswordServices = resetPasswordServices;
        Message = resetPasswordServices.Count() == 1 ? _resetPasswordServices.First().RequestMessage : "Chose method and enter your data";
    }

    public override async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            IResetPasswordService? service = _resetPasswordServices.FirstOrDefault(s => s.ServiceName == Input.Method);
            if (service != null)
            {
                Data = Activator.CreateInstance(service.DataModelType);
                if (Data != null)
                {
                    string prefix = "Input." + service.ServiceName + ".";
                    int len = prefix.Length;
                    foreach (KeyValuePair<string, StringValues> p in Request.Form.Where(p => p.Key.StartsWith(prefix)))
                    {
                        var prop = service.DataModelType.GetProperty(p.Key[len..]);
                        prop?.SetValue(Data, Convert.ChangeType(p.Value[0], prop.PropertyType));
                    }

                    if (await service.SendResetPasswordRequest(PageContext, Data, "Input."))
                    {
                        return RedirectToPage("./ForgotPasswordConfirmation", new { method = Input.Method });
                    }
                }
            }
        }

        return Page();
    }
}
