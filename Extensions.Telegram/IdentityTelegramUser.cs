using Microsoft.AspNetCore.Identity;

namespace Boa.Identity.Telegram;

/// <summary>
/// The default implementation of <see cref="IdentityTelegramUser{TKey}"/> which uses a string as a primary key.
/// </summary>
public class IdentityTelegramUser : IdentityTelegramUser<string>
{
    /// <summary>
    /// Initializes a new instance of <see cref="IdentityTelegramUser"/>.
    /// </summary>
    /// <remarks>
    /// The Id property is initialized to form a new GUID string value.
    /// </remarks>
    public IdentityTelegramUser()
    {
        Id = Guid.NewGuid().ToString();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="IdentityTelegramUser"/>.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <remarks>
    /// The Id property is initialized to form a new GUID string value.
    /// </remarks>
    public IdentityTelegramUser(string userName) : this()
    {
        UserName = userName;
    }
}

/// <summary>
/// Represents a user in the identity system
/// </summary>
/// <typeparam name="TKey">The type used for the primary key for the user.</typeparam>
public class IdentityTelegramUser<TKey> : IdentityUser<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Initializes a new instance of <see cref="IdentityTelegramUser{TKey}"/>.
    /// </summary>
    public IdentityTelegramUser() { }

    /// <summary>
    /// Initializes a new instance of <see cref="IdentityTelegramUser{TKey}"/>.
    /// </summary>
    /// <param name="userName">The user name.</param>
    public IdentityTelegramUser(string userName) : this()
    {
        UserName = userName;
    }

    /// <summary>
    /// Gets or sets the Telegram Id for this user.
    /// </summary>
    [ProtectedPersonalData]
    public long? TelegramId { get; set; }
}