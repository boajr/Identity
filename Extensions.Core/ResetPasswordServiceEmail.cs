using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace Boa.Identity;

/// <summary>
/// The data model used by <see cref="ResetPasswordServiceEmail"/>.
/// </summary>
public sealed class ResetPasswordServiceEmailDataModel
{
    /// <summary>
    /// Email address of user that request password reset.
    /// </summary>
    [EmailAddress]
    [Display(Prompt = "name@example.com")]
    public string Email { get; set; } = default!;
}

/// <summary>
/// A <see cref="ResetPasswordService{}"/> implementation that allow reset password by mail.
/// </summary>
public abstract class ResetPasswordServiceEmail : ResetPasswordServiceEmail<IdentityUser>
{
    public ResetPasswordServiceEmail(IObjectModelValidator modelValidator,
                                     UserManager<IdentityUser> userManager)
        : base(modelValidator, userManager)
    { }
}

/// <summary>
/// A <see cref="ResetPasswordService{}"/> implementation that allow reset password by mail.
/// </summary>
/// <typeparam name="TUser">The type of the user objects.</typeparam>
public abstract class ResetPasswordServiceEmail<TUser> : ResetPasswordService<ResetPasswordServiceEmailDataModel>
    where TUser : class
{
    protected readonly UserManager<TUser> _userManager;

    /// <inheritdoc />
    public override string ServiceName => "Email";

    /// <inheritdoc />
    public sealed override string RequestMessage => "Enter your email";

    //public sealed override string LongMessage => "Enter your email address<br>to receive an email with a reset password link";

    /// <inheritdoc />
    public sealed override string ConfirmationMessage => "Please check your email to reset your password";

    /// <summary>
    /// Initializes a new instance of <see cref="ResetPasswordServiceEmail"/>.
    /// </summary>
    /// <param name="modelValidator">The <see cref="IObjectModelValidator"/> used for validating the
    /// <see cref="ResetPasswordServiceEmailDataModel"/> object.</param>
    /// <param name="userManager">The <see cref="UserManager{TUser}"/> to retrieve user properties from.</param>
    public ResetPasswordServiceEmail(IObjectModelValidator modelValidator, UserManager<TUser> userManager)
        : base(modelValidator)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    /// <summary>
    /// Asynchronously process the reset password request after that <paramref name="data"/> is validated.
    /// The class ModelState is updated with the validation results.
    /// </summary>
    /// <param name="data">A DataModelType object with data entered by user.</param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> that, when completed, returns <c>true</c> if the reset password request
    /// is send, otherwise returns <c>false</c>.
    /// </returns>
    protected sealed override async Task<bool> ProcessAsync(ResetPasswordServiceEmailDataModel data)
    {
        if (!ModelState.IsValid) { 
            return false;
        }

        TUser? user = await _userManager.FindByEmailAsync(data.Email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            // Don't reveal that the user does not exist or is not confirmed
            return true;
        }

        // For more information on how to enable account confirmation and password reset please
        // visit https://go.microsoft.com/fwlink/?LinkID=532713
        string code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        string callbackUrl = GetCallbackUrl(code);

        await SendEmailAsync(
            data.Email,
            "Reset Password",
            $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        return true;
    }

    /// <summary>
    /// Get the url of the reset password page to send to user.
    /// </summary>
    /// <param name="token">The token to send to user.</param>
    /// <returns>A string with the url to send to user.</returns>
    protected abstract string GetCallbackUrl(string token);

    /// <summary>
    /// Sends an email to the specified <paramref name="email"/> address as an asynchronous operation.
    /// </summary>
    /// <param name="email">The email address to send email.</param>
    /// <param name="subject">The subject of the eamil.</param>
    /// <param name="htmlMessage">The body of the email in html format.</param>
    /// <returns>A <see cref="Task"/> that completes when mail is sent.</returns>
    protected abstract Task SendEmailAsync(string email, string subject, string htmlMessage);
}
