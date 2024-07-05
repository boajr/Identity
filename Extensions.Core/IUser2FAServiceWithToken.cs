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
    /// The number of seconds that must pass between one code request and another.
    /// </summary>
    int ResendSeconds { get; }

    /// <summary>
    /// Returns the number of seconds to wait before send another token for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user waiting for a token.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the number of seconds to wait
    /// before send another token.
    /// </returns>
    Task<int> TimeToWaitAsync(UserManager<TUser> manager, TUser user);

    /// <summary>
    /// Generates and send a token for the specified <paramref name="user"/> and <paramref name="purpose"/>.
    /// </summary>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user a token should be generated for.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing a <see cref="Send2FATokenResult"/>
    /// indicating the result of the operation.
    /// </returns>
    Task<Send2FATokenResult> SendTokenAsync(UserManager<TUser> manager, TUser user);

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
