using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace Boa.Identity;

public static class UserManagerExtensions
{
    private const string InternalLoginProvider = "[BoaUserStore]";
    private const string TwoFactorProviderTokenName = "TwoFactorProvider";

    /// <summary>
    /// Sets the two factor authentication provider for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose authenticator provider should be set.</param>
    /// <param name="provider">The two factor authentication provider to set.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public static async Task<IdentityResult> SetTwoFactorProviderAsync<TUser>(this UserManager<TUser> userManager, TUser user, string provider) where TUser : class
    {
        ThrowIfDisposed(userManager);
        ArgumentNullException.ThrowIfNull(user);

        var store = GetAuthenticationTokenStore(userManager);
        await store.SetTokenAsync(user, InternalLoginProvider, TwoFactorProviderTokenName, provider, GetCancellationToken(userManager)).ConfigureAwait(false);
        //await userManager.UpdateSecurityStampInternal(user).ConfigureAwait(false);
        return await userManager.UpdateAsync(user).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the two factor authentication provider for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose authenticator provider should be deleted.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public static async Task<IdentityResult> RemoveTwoFactorProviderAsync<TUser>(this UserManager<TUser> userManager, TUser user) where TUser : class
    {
        ThrowIfDisposed(userManager);
        ArgumentNullException.ThrowIfNull(user);

        var store = GetAuthenticationTokenStore(userManager);
        await store.RemoveTokenAsync(user, InternalLoginProvider, TwoFactorProviderTokenName, GetCancellationToken(userManager)).ConfigureAwait(false);
        //await userManager.UpdateSecurityStampInternal(user).ConfigureAwait(false);
        return await userManager.UpdateAsync(user).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the two factor authentication provider for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose authenticator provider should be retrived.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the two factor authentication provider for the specified <paramref name="user"/>.</returns>
    public static Task<string?> GetTwoFactorProviderAsync<TUser>(this UserManager<TUser> userManager, TUser user) where TUser : class
    {
        ThrowIfDisposed(userManager);
        ArgumentNullException.ThrowIfNull(user);

        var store = GetAuthenticationTokenStore(userManager);
        return store.GetTokenAsync(user, InternalLoginProvider, TwoFactorProviderTokenName, GetCancellationToken(userManager));
    }

    /// <summary>
    /// Get the token provider associated to <paramref name="providerName"/>.
    /// </summary>
    /// <param name="providerName">The name of the provider to retrieve.</param>
    /// <returns>The <see cref="IUserTwoFactorTokenProvider<>"/> to <paramref name="providerName"/>.</returns>
    public static IUserTwoFactorTokenProvider<TUser>? GetTokenProvider<TUser>(this UserManager<TUser> userManager, string providerName) where TUser : class
    {
        ThrowIfDisposed(userManager);

        var providers = GetTokenProviders(userManager);
        if (providers == null)
            return null;

        return providers[providerName];
    }



    private static void ThrowIfDisposed<TUser>(UserManager<TUser> userManager) where TUser : class
    {
        MethodInfo throwIfDisposed = userManager.GetType().GetMethod("ThrowIfDisposed", BindingFlags.NonPublic | BindingFlags.Instance)
                                     ?? throw new ObjectDisposedException(userManager.GetType().Name);
        throwIfDisposed.Invoke(userManager, null);
    }

    private static IUserAuthenticationTokenStore<TUser> GetAuthenticationTokenStore<TUser>(UserManager<TUser> userManager) where TUser : class
    {
        if (userManager
            .GetType()
            .GetProperty("Store", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetMethod?
            .Invoke(userManager, null) is not IUserAuthenticationTokenStore<TUser> store)
        {
            throw new NotSupportedException("Store does not implement IUserAuthenticationTokenStore<TUser>.");
        }
        return store;
    }

    private static CancellationToken GetCancellationToken<TUser>(UserManager<TUser> userManager) where TUser : class
    {
        if (userManager
            .GetType()
            .GetProperty("CancellationToken", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetMethod?
            .Invoke(userManager, null) is not CancellationToken cancellationToken)
        {
            throw new MissingMemberException("UserManager does not contain CancellationToken property.");
        }
        return cancellationToken;
    }

    private static Dictionary<string, IUserTwoFactorTokenProvider<TUser>> GetTokenProviders<TUser>(UserManager<TUser> userManager) where TUser : class
    {
        if (userManager
            .GetType()
            .GetField("_tokenProviders", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(userManager) is not Dictionary<string, IUserTwoFactorTokenProvider<TUser>> _tokenProviders)
        {
            throw new MissingMemberException("UserManager does not contain _tokenProviders field.");
        }
        return _tokenProviders;
    }
}
