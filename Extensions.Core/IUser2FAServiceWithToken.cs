using Microsoft.AspNetCore.Identity;

namespace Boa.Identity;

public interface IUser2FAServiceWithToken<TUser> : IUser2FAService<TUser>
    where TUser : class
{
    /// <summary>
    /// A flag indicating whether <see cref="SendTokenAsync"/> needs to be called to send tokens to users.
    /// </summary>
    bool NeedToSendToken { get; }

    /// <summary>
    /// Generates and send a token for the specified <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user a token should be generated for.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating the result
    /// of the operation.
    /// The task will return true if the token is sended to the <paramref name="user"/>, otherwise false.
    /// </returns>
    Task<bool> SendTokenAsync(UserManager<TUser> manager, TUser user);

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="token"/> is valid for the given
    /// <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user a token should be validated for.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the a flag indicating the result
    /// of validating the <paramref name="token"> for the specified </paramref><paramref name="user"/> and <paramref name="purpose"/>.
    /// The task will return true if the token is valid, otherwise false.
    /// </returns>
    Task<bool> ValidateTokenAsync(string token, UserManager<TUser> manager, TUser user);
}
