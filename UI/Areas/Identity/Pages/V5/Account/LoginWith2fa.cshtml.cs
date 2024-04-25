// Manca da fare
//   1. il resend del codice



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Boa.Identity.UI.V5.Pages.Account.Internal;

/// <summary>
///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
///     directly from your code. This API may change or be removed in future releases.
/// </summary>
[AllowAnonymous]
[IdentityUI(typeof(LoginWith2faModel<>))]
public abstract class LoginWith2faModel : PageModel
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public bool RememberMe { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Display(Name = "Remember authentication service")]
    public bool RememberService { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public bool ChooseService { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public List<SelectListItem>? ServicesList { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public bool CanResendToken { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator service")]
        public string TwoFactorService { get; set; } = default!;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; } = default!;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public virtual Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null) => throw new NotImplementedException();

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public virtual Task<IActionResult> OnPostAsync(bool rememberMe, bool chooseService, bool rememberService, string? returnUrl = null) => throw new NotImplementedException();
}

internal sealed class LoginWith2faModel<TUser> : LoginWith2faModel where TUser : class
{
    private readonly IEnumerable<IUser2FAService<TUser>> _user2FAServices;
    private readonly SignInManager<TUser> _signInManager;
    private readonly UserManager<TUser> _userManager;
    private readonly ILogger<LoginWith2faModel> _logger;

    public LoginWith2faModel(IEnumerable<IUser2FAService<TUser>> user2FAServices,
                             SignInManager<TUser> signInManager,
                             UserManager<TUser> userManager,
                             ILogger<LoginWith2faModel> logger)
    {
        _user2FAServices = user2FAServices;
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task<IActionResult> OnGetAsync(bool rememberMe, [StringSyntax(StringSyntaxAttribute.Uri)] string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        RememberMe = rememberMe;

        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync()
                   ?? throw new InvalidOperationException($"Unable to load two-factor authentication user.");

        List<IUser2FAService<TUser>> services = await GetValidTwoFactorServicesAsync(user);
        if (services.Count == 0)
        {
            // TODO - scegliere una di queste due funzioni
            // 1. estrarre una lista di tutti i servizi di 2fa e una volta scelto girare l'utente alla pagina di configurazione
            // 2. inviare l'utente alla pagina di configurazione del 2fa e non permettergli di uscire finche non l'ha configurato
            return Page();
        }

        IUser2FAService<TUser>? service = null;
        if (services.Count > 1)
        {
            string? provider = await _userManager.GetTwoFactorProviderAsync(user);
            if (provider != null)
            {
                foreach (var s in services)
                {
                    if (s.ServiceName == provider)
                    {
                        service = s;
                        break;
                    }
                }
            }
        }
        else
        {
            service = services[0];
        }

        if (service == null)
        {
            ChooseService = true;
            ServicesList = services.Select(s => new SelectListItem(s.ServiceName, s.ServiceName)).ToList();
            return Page();
        }

        if (service is IUser2FAServiceWithToken<TUser> serviceWithToken)
        {
            if (serviceWithToken.NeedToSendToken)
            {
                await serviceWithToken.SendTokenAsync(_userManager, user);
                CanResendToken = true;
            }
        }
        else if (service is IUser2FAServiceWithWebService<TUser> serviceWithWebService)
        {

        }
        else
        {
            throw new InvalidOperationException($"Unable to manage " + service.GetType().Name + " derived class.");
        }

        Input = new()
        {
            TwoFactorService = service.ServiceName
        };
        return Page();
    }

    public override async Task<IActionResult> OnPostAsync(bool rememberMe, bool chooseService, bool rememberService, string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        RememberMe = rememberMe;
        RememberService = rememberService;

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync()
                   ?? throw new InvalidOperationException($"Unable to load two-factor authentication user.");

        var tfp = ModelState["Input.TwoFactorService"];
        if (chooseService)
        {
            if (tfp == null || tfp.ValidationState != ModelValidationState.Valid)
            {
                ChooseService = true;
                ServicesList = (await GetValidTwoFactorServicesAsync(user))?.Select(p => new SelectListItem(p.ServiceName, p.ServiceName)).ToList();
            }
            else
            {
                if (await GetTwoFactorServiceAsync(user, Input.TwoFactorService) is IUser2FAServiceWithToken<TUser> serviceWithToken)
                {
                    if (serviceWithToken.NeedToSendToken)
                    {
                        await serviceWithToken.SendTokenAsync(_userManager, user);
                        CanResendToken = true;
                    }
                }
            }

            ModelState["Input.TwoFactorCode"]?.Errors.Clear();

            return Page();
        }

        if (tfp != null && tfp.ValidationState == ModelValidationState.Invalid && string.IsNullOrEmpty(Input.TwoFactorService))
        {
            tfp.Errors.Clear();
            tfp.ValidationState = ModelValidationState.Valid;
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        // verifica se devo validare il codice su uno specifico servizio
        IUser2FAServiceWithToken<TUser>? service = null;
        foreach (var s in await GetValidTwoFactorServicesAsync(user))
        {
            if (s.ServiceName == Input.TwoFactorService && s is IUser2FAServiceWithToken<TUser> swt)
            {
                service = swt;
                break;
            }
        }

        var result = service == null
                   ? await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine)
                   : await _signInManager.TwoFactorSignInWithTokenAsync(service, authenticatorCode, rememberMe, Input.RememberMachine, rememberService);

        //await _userManager.GetUserIdAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation(LoggerEventIds.UserLoginWith2FA, "User logged in with 2fa.");
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning(LoggerEventIds.UserLockout, "User account locked out.");
            return RedirectToPage("./Lockout");
        }
        else
        {
            _logger.LogWarning(LoggerEventIds.InvalidAuthenticatorCode, "Invalid authenticator code entered.");
            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return Page();
        }
    }

    private async Task<List<IUser2FAService<TUser>>> GetValidTwoFactorServicesAsync(TUser user)
    {
        List<IUser2FAService<TUser>> services = [];
        foreach (var service in _user2FAServices)
        {
            if (await service.IsSuitableAsync(_userManager, user))
            {
                services.Add(service);
            }
        }
        return services;
    }

    private async Task<IUser2FAService<TUser>?> GetTwoFactorServiceAsync(TUser user, string service)
    {
        foreach (var s in _user2FAServices)
        {
            if (s.ServiceName == service && await s.IsSuitableAsync(_userManager, user))
            {
                return s;
            }
        }
        return null;
    }
}
