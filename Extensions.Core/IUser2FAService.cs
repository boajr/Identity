using Microsoft.AspNetCore.Identity;

namespace Boa.Identity;

public interface IUser2FAService<TUser> where TUser : class
{
    /// <summary>
    /// The name to use to identifing the service
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Message to show to users when they chose the service to authenticate.
    /// </summary>
    string RequestMessage { get; }

    /// <summary>
    /// Returns a flag indicating whether the service is suitable for two-factor authentication
    /// for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user that need to use the service.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the a flag indicating if the
    /// specified <paramref name="user"/> can use this service for authenticating.
    /// The task will return true if the service is suitable for authentication, otherwise false.
    /// </returns>
    Task<bool> IsSuitableAsync(UserManager<TUser> manager, TUser user);
}
