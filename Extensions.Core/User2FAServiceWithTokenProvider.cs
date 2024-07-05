using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace Boa.Identity;

public abstract class User2FAServiceWithTokenProvider<TUser> : IUser2FAServiceWithToken<TUser>
    where TUser : class
{
    private readonly IServiceProvider _serviceProvider;

    public string ServiceName { get; }

    public int ResendSeconds { get; } = 60;

    abstract public string RequestMessage { get; }

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



    public User2FAServiceWithTokenProvider(IServiceProvider serviceProvider, IConfiguration configuration, string providerName)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        ServiceName = providerName;
        if (int.TryParse(configuration[$"Boa.Identity:User2FAServiceWithTokenProvider.TimeToWait:{ServiceName}"], out int val))
        {
            ResendSeconds = val;
        }
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

    public async Task<int> TimeToWaitAsync(UserManager<TUser> manager, TUser user)
    {
        // verifica se sono trascorsi i secondi di attesa dall'invio precedente
        var dt = await manager.GetLast2FATokenTimeAsync(user);
        if (!dt.HasValue)
        {
            return 0;
        }

        var next = dt.Value.AddSeconds(ResendSeconds);
        var now = DateTime.UtcNow;
        if (next <= now)
        {
            return 0;
        }

        return (int)(next - now).TotalSeconds;
    }

    public async Task<Send2FATokenResult> SendTokenAsync(UserManager<TUser> manager, TUser user)
    {
        var provider = manager.GetTokenProvider(ServiceName)
            ?? throw new NotImplementedException("The IUserTwoFactorTokenProvider<TUser> associated with this service isn't registered.");

        if (!NeedToSendToken)
        {
            return Send2FATokenResult.NotNeeded;
        }

        int timeToWait = await TimeToWaitAsync(manager, user);
        if (timeToWait > 0)
        {
            return Send2FATokenResult.Wait(timeToWait);
        }

        var token = await provider.GenerateAsync("TwoFactor", manager, user).ConfigureAwait(false);
        var result = await ProcessSendTokenAsync(token, manager, user).ConfigureAwait(false);
        if (result.Succeeded)
        {
            await manager.SetLast2FATokenTimeAsync(user);
        }
        return result;
    }

    /// <summary>
    /// Synchronously process the send token request after that the <paramref name="token"/> is generated.
    /// </summary>
    /// <param name="token">The generated token.</param>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user to send the token.</param>
    /// <returns>
    /// A <see cref="Send2FATokenResult"/> indicating the result of the operation.
    /// </returns>
    protected virtual Send2FATokenResult ProcessSendToken(string token, UserManager<TUser> manager, TUser user)
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
    /// A <see cref="Task"/> that represents the asynchronous operation, containing a <see cref="Send2FATokenResult"/>
    /// indicating the result of the operation.
    /// </returns>
    /// <remarks>By default this calls into <see cref="ProcessSendToken"/>.</remarks>
    protected virtual Task<Send2FATokenResult> ProcessSendTokenAsync(string token, UserManager<TUser> manager, TUser user)
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
