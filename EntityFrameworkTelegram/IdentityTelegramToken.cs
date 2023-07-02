using Microsoft.AspNetCore.Identity;

namespace Boa.Identity.EntityFrameworkTelegram;

/// <summary>
/// Represents a token for a Telegram user.
/// </summary>
public class IdentityTelegramToken
{
    /// <summary>
    /// Gets or sets the primary key of the user that the token belongs to.
    /// </summary>
    public virtual long TelegramId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the LoginProvider this token is from.
    /// </summary>
    public virtual string LoginProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the token.
    /// </summary>
    public virtual string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the token value.
    /// </summary>
    [ProtectedPersonalData]
    public virtual string? Value { get; set; }
}
