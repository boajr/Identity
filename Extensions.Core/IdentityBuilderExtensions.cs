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

        builder.Services.AddScoped(typeof(IResetPasswordService), implementationType);
        return builder;
    }

    /// <summary>
    /// Adds a <see cref="IResetPasswordService"/> used to allow users to reset their password.
    /// </summary>
    /// <typeparam name="TResetPasswordService">The type of the reset password service to add.</typeparam>
    /// <param name="builder">The current <see cref="IdentityBuilder"/> instance.</param>
    /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IdentityBuilder AddBoaResetPasswordService<TResetPasswordService>(this IdentityBuilder builder) where TResetPasswordService : IResetPasswordService
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddBoaResetPasswordService(typeof(TResetPasswordService));
        return builder;
    }
}
