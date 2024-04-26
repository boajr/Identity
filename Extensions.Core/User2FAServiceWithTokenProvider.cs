using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Boa.Identity;

public abstract class User2FAServiceWithTokenProvider<TUser> : IUser2FAServiceWithToken<TUser>
    where TUser : class
{
    private readonly IServiceProvider _serviceProvider;

    public string ServiceName { get; }

    abstract public bool NeedToSendToken { get; }


    /// <summary>
    /// Gets the <see cref="IStringLocalizer"/> for localized strings.
    /// </summary>
    protected IStringLocalizer Localizer
    {
        get
        {
            _localizer ??= new IdentityStringLocalizer(_serviceProvider, "Boa.Identity.User2FAService");
            return _localizer;
        }
    }
    private IStringLocalizer? _localizer;



    public User2FAServiceWithTokenProvider(IServiceProvider serviceProvider, string providerName)
    {
        ServiceName = providerName;
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<bool> IsSuitableAsync(UserManager<TUser> manager, TUser user)
    {
        var provider = manager.GetTokenProvider(ServiceName);
        if (provider == null)
        {
            return false;
        }

        return await provider.CanGenerateTwoFactorTokenAsync(manager, user).ConfigureAwait(false);
    }

    public async Task<bool> SendTokenAsync(UserManager<TUser> manager, TUser user)
    {
        var provider = manager.GetTokenProvider(ServiceName);
        if (provider == null)
        {
            return false;
        }

        var token = await provider.GenerateAsync("TwoFactor", manager, user).ConfigureAwait(false);

        return await ProcessSendTokenAsync(token, manager, user).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously process the send token request after that the <paramref name="token"/> is generated.
    /// </summary>
    /// <param name="token">The generated token.</param>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user to send the token.</param>
    /// <returns>
    /// Returns <c>true</c> if the <paramref name="token"/> is sent, otherwise returns <c>false</c>.
    /// </returns>
    protected virtual bool ProcessSendToken(string token, UserManager<TUser> manager, TUser user)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Asynchronously process the send token request after that the <paramref name="token"/> is generated.
    /// </summary>
    /// <param name="token">The generated token.</param>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user to send the token.</param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> that, when completed, returns <c>true</c> if the <paramref name="token"/>
    /// is sent, otherwise returns <c>false</c>.
    /// </returns>
    /// <remarks>By default this calls into <see cref="ProcessSendToken"/>.</remarks>
    protected virtual Task<bool> ProcessSendTokenAsync(string token, UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(ProcessSendToken(token, manager, user));
    }

    public async Task<bool> ValidateTokenAsync(string token, UserManager<TUser> manager, TUser user)
    {
        var provider = manager.GetTokenProvider(ServiceName);
        if (provider == null)
        {
            return false;
        }

        return await provider.ValidateAsync("TwoFactor", token, manager, user).ConfigureAwait(false);
    }
}
