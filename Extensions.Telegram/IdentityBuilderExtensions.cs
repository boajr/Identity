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
    /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
    public static IdentityBuilder AddBoaResetPasswordServiceTelegram(this IdentityBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (FindGenericBaseType(builder.UserType, typeof(IdentityTelegramUser<>)) == null)
        {
            throw new InvalidOperationException("AddBoaResetPasswordServiceTelegram can only be called with a user that derives from IdentityTelegramUser<TKey>.");
        }

        builder.AddBoaResetPasswordService(typeof(ResetPasswordServiceTelegram<>).MakeGenericType(builder.UserType));
        builder.Services.TryAddSingleton<ITelegramBotHandler, TelegramBotHandler>();
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
