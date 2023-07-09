using Boa.Identity.EntityFrameworkTelegram;
using Boa.Identity.Telegram;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods to <see cref="IdentityBuilder"/> for adding entity framework stores.
/// </summary>
public static class IdentityBuilderExtensions
{
    /// <summary>
    /// Adds an Entity Framework implementation of identity information stores.
    /// </summary>
    /// <typeparam name="TContext">The Entity Framework database context to use.</typeparam>
    /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
    /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
    public static IdentityBuilder AddBoaEntityFrameworkTelegramStores<TContext>(this IdentityBuilder builder)
        where TContext : DbContext
    {
        // Aggiungo la UserManager con i comandi aggiuntivi per gestire la parte di Telegram
        // builder.AddUserManager<TelegramUserManager<TUser>>();
        var userManagerType = typeof(UserManager<>).MakeGenericType(builder.UserType);
        var customType = typeof(TelegramUserManager<>).MakeGenericType(builder.UserType);
        if (userManagerType != customType)
        {
            builder.Services.AddScoped(customType, services => services.GetRequiredService(userManagerType));
        }
        builder.Services.AddScoped(userManagerType, customType);

        // aggiungo gli store che gestiscono i comandi aggiuntivi per la parte di Telegram
        AddStores(builder.Services, builder.UserType, builder.RoleType, typeof(TContext));
        return builder;
    }

    private static void AddStores(IServiceCollection services, Type userType, Type? roleType, Type contextType)
    {
        var identityUserType = FindGenericBaseType(userType, typeof(IdentityTelegramUser<>))
            ?? throw new InvalidOperationException("AddBoaEntityFrameworkTelegramStores can only be called with a user that derives from IdentityTelegramUser<TKey>.");
        var keyType = identityUserType.GenericTypeArguments[0];

        if (roleType != null)
        {
            if (FindGenericBaseType(roleType, typeof(IdentityRole<>)) == null)
            {
                throw new InvalidOperationException("AddBoaEntityFrameworkTelegramStores can only be called with a role that derives from IdentityRole<TKey>.");
            }

            Type userStoreType;
            Type roleStoreType;
            var identityContext = FindGenericBaseType(contextType, typeof(IdentityDbContext<,,,,,,,>));
            var telegramContext = FindGenericBaseType(contextType, typeof(Boa.Identity.EntityFrameworkTelegram.IdentityDbContext<,>));
            if (identityContext == null || telegramContext == null)
            {
                // If its a custom DbContext, we can only add the default POCOs
                userStoreType = typeof(TelegramUserStore<,,,>).MakeGenericType(userType, roleType, contextType, keyType);
                roleStoreType = typeof(RoleStore<,,>).MakeGenericType(roleType, contextType, keyType);
            }
            else
            {
                userStoreType = typeof(TelegramUserStore<,,,,,,,,,>).MakeGenericType(userType, roleType, contextType,
                    identityContext.GenericTypeArguments[2],
                    identityContext.GenericTypeArguments[3],
                    identityContext.GenericTypeArguments[4],
                    identityContext.GenericTypeArguments[5],
                    identityContext.GenericTypeArguments[7],
                    identityContext.GenericTypeArguments[6],
                    telegramContext.GenericTypeArguments[1]);
                roleStoreType = typeof(RoleStore<,,,,>).MakeGenericType(roleType, contextType,
                    identityContext.GenericTypeArguments[2],
                    identityContext.GenericTypeArguments[4],
                    identityContext.GenericTypeArguments[6]);
            }
            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
        }
        else
        {   // No Roles
            Type userStoreType;
            var identityContext = FindGenericBaseType(contextType, typeof(IdentityUserContext<,,,,>));
            var telegramContext = FindGenericBaseType(contextType, typeof(Boa.Identity.EntityFrameworkTelegram.IdentityUserContext<,>));
            if (identityContext == null || telegramContext == null)
            {
                // If its a custom DbContext, we can only add the default POCOs
                userStoreType = typeof(TelegramUserOnlyStore<,,>).MakeGenericType(userType, contextType, keyType);
            }
            else
            {
                userStoreType = typeof(TelegramUserOnlyStore<,,,,,,>).MakeGenericType(userType, contextType,
                    identityContext.GenericTypeArguments[1],
                    identityContext.GenericTypeArguments[2],
                    identityContext.GenericTypeArguments[3],
                    identityContext.GenericTypeArguments[4],
                    telegramContext.GenericTypeArguments[1]);
            }
            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
        }
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
