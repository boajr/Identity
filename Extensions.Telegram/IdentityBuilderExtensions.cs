using Boa.Identity;
using Boa.Identity.Telegram;
using Boa.TelegramBotService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods to <see cref="IdentityBuilder"/> for adding telegram functionality.
/// </summary>
public static class IdentityBuilderExtensions
{
    /// <summary>
    /// Adds an <see cref="IResetPasswordService"/> that use telegram to reset password.
    /// </summary>
    /// <remarks>
    /// In order to use this service, it's needed to add a Boa.TelegramBotService.
    /// </remarks>
    /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
    public static IdentityBuilder AddBoaResetPasswordServiceTelegram(this IdentityBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (FindGenericBaseType(builder.UserType, typeof(IdentityTelegramUser<>)) == null)
        {
            throw new InvalidOperationException("AddBoaResetPasswordServiceTelegram can only be called with a user that derives from IdentityTelegramUser<TKey>.");
        }

        builder.AddBoaResetPasswordService(typeof(ResetPasswordServiceTelegram<>).MakeGenericType(builder.UserType));
        builder.Services.TryAddScoped(typeof(ITelegramBotHandler), typeof(TelegramBotHandler<>).MakeGenericType(builder.UserType));
        return builder;
    }

    /// <summary>
    /// Adds a two factor authentication service that uses the <see cref="TelegramTokenProvider<>"/> as token provider.
    /// </summary>
    /// <remarks>
    /// In order to use this service, it's needed to add a Boa.TelegramBotService.
    /// </remarks>
    /// <param name="builder">The current <see cref="IdentityBuilder"/> instance.</param>
    /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
    public static IdentityBuilder AddBoaUser2FAServiceWithTelegram(this IdentityBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (FindGenericBaseType(builder.UserType, typeof(IdentityTelegramUser<>)) == null)
        {
            throw new InvalidOperationException("AddBoaUser2FAServiceWithTelegram can only be called with a user that derives from IdentityTelegramUser<TKey>.");
        }

        builder.AddTokenProvider("Telegram", typeof(TelegramTokenProvider<>).MakeGenericType(builder.UserType));
        builder.AddBoaUser2FAService(typeof(User2FAServiceWithTelegram<>).MakeGenericType(builder.UserType));
        builder.Services.TryAddScoped(typeof(ITelegramBotHandler), typeof(TelegramBotHandler<>).MakeGenericType(builder.UserType));
        return builder;
    }

    private static Type? FindGenericBaseType(Type currentType, Type genericBaseType)
    {
        Type? type = currentType;
        while (type != null)
        {
            var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
            if (genericType != null && genericType == genericBaseType)
            {
                return type;
            }
            type = type.BaseType;
        }
        return null;
    }
}
