using Microsoft.AspNetCore.Mvc;

namespace Boa.Identity;

/// <summary>
/// Provides an abstraction for a service to reset password
/// </summary>
public interface IResetPasswordService
{
    /// <summary>
    /// Name of the service showed to users when chosing reset method.
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Data Model needed by service for managing requests.
    /// </summary>
    Type DataModelType { get; }

    /// <summary>
    /// Message to show to users when they chose the service to reset their password.
    /// </summary>
    string RequestMessage { get; }

    /// <summary>
    /// Message to show to users after reset password requests are send.
    /// </summary>
    string ConfirmationMessage { get; }

    /// <summary>
    /// Send a reset password request to user as an asyncronous operation.
    /// </summary>
    /// <param name="context">The <see cref="ActionContext"/> associated with the current request.</param>
    /// <param name="data">A DataModelType object with data entered by user.</param>
    /// <param name="prefix">
    /// The model prefix. Used to map the model object to entries in <paramref name="ModelState"/>.
    /// To this prefix will be appended the <see cref="ServiceName"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> that, when completed, returns <c>true</c> if the reset password request
    /// is send, otherwise returns <c>false</c>.
    /// </returns>
    Task<bool> SendResetPasswordRequest(ActionContext context, object data, string prefix = "");
}
