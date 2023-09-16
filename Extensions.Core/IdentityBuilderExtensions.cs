using Boa.Identity;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Helper functions for configuring identity services.
/// </summary>
public static class IdentityBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="IResetPasswordService"/> used to allow users to reset their password.
    /// </summary>
    /// <param name="builder">The current <see cref="IdentityBuilder"/> instance.</param>
    /// <param name="implementationType">The type of the reset password service to add.</param>
    /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IdentityBuilder AddBoaResetPasswordService(this IdentityBuilder builder,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (implementationType == null)
        {
            throw new ArgumentNullException(nameof(implementationType));
        }

        if (!typeof(IResetPasswordService).IsAssignableFrom(implementationType))
        {
            throw new InvalidOperationException(implementationType.Name + " does not implement IResetPasswordService.");
        }

        builder.Services.AddScoped(typeof(IResetPasswordService), implementationType);
        return builder;
    }

    /// <summary>
    /// Adds a <see cref="IResetPasswordService"/> used to allow users to reset their password.
    /// </summary>
    /// <typeparam name="TService">The type of the reset password service to add.</typeparam>
    /// <param name="builder">The current <see cref="IdentityBuilder"/> instance.</param>
    /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IdentityBuilder AddBoaResetPasswordService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IdentityBuilder builder) where TService : IResetPasswordService
        => AddBoaResetPasswordService(builder, typeof(TService));

    /// <summary>
    /// Adds a two factor authentication service for the <see cref="IdentityBuilder.UserType"/>.
    /// </summary>
    /// <param name="builder">The current <see cref="IdentityBuilder"/> instance.</param>
    /// <param name="implementationType">The type of the <see cref="IUserTwoFactorTokenProvider{TUser}"/> to add.</param>
    /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IdentityBuilder AddBoaUser2FAService(this IdentityBuilder builder, 
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (implementationType == null)
        {
            throw new ArgumentNullException(nameof(implementationType));
        }

        Type type = typeof(IUser2FAService<>).MakeGenericType(builder.UserType);
        if (!type.IsAssignableFrom(implementationType))
        {
            throw new InvalidOperationException(implementationType.Name + " does not implement IUser2FAService<"+ builder.UserType.Name + ">.");
        }

        builder.Services.AddTransient(type, implementationType);
        return builder;
    }

    /// <summary>
    /// Adds a two factor authentication service.
    /// </summary>
    /// <typeparam name="TService">The type of the two factor authentication service to add.</typeparam>
    /// <param name="builder">The current <see cref="IdentityBuilder"/> instance.</param>
    /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IdentityBuilder AddBoaUser2FAService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IdentityBuilder builder) where TService : class
        => AddBoaUser2FAService(builder, typeof(TService));

    /// <summary>
    /// Adds a two factor authentication service that uses the <see cref="AuthenticatorTokenProvider<>"/> as token provider.
    /// </summary>
    /// <param name="builder">The current <see cref="IdentityBuilder"/> instance.</param>
    /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IdentityBuilder AddBoaUser2FAServiceWithAuthenticator(this IdentityBuilder builder)
        => AddBoaUser2FAService(builder, typeof(User2FAServiceWithAuthenticator<>).MakeGenericType(builder.UserType));
}
