using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;

namespace Boa.Identity;

/// <summary>
/// Provides an abstraction for a service to reset password implementing the
/// <see cref="IResetPasswordService"/> interface.
/// </summary>
public abstract class ResetPasswordService<TDataModel> : IResetPasswordService
    where TDataModel : class
{
    private readonly IServiceProvider _serviceProvider;
    protected readonly IObjectModelValidator ModelValidator;
    private ActionContext _context = default!;

    /// <summary>
    /// Gets the <see cref="Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor"/> for the selected action.
    /// </summary>
    protected ActionDescriptor ActionDescriptor { get { return _context.ActionDescriptor; } }

    /// <summary>
    /// Gets the <see cref="Microsoft.AspNetCore.Http.HttpContext"/> for the current request.
    /// </summary>
    protected HttpContext HttpContext { get { return _context.HttpContext; } }

    /// <summary>
    /// Gets the <see cref="ModelStateDictionary"/>.
    /// </summary>
    protected ModelStateDictionary ModelState { get { return _context.ModelState; } }

    /// <summary>
    /// Gets the <see cref="Microsoft.AspNetCore.Routing.RouteData"/> for the current request.
    /// </summary>
    protected RouteData RouteData { get { return _context.RouteData; } }

    /// <summary>
    /// Gets the <see cref="Microsoft.Extensions.Localization.IStringLocalizer"/> for localized strings.
    /// </summary>
    protected IStringLocalizer Localizer
    {
        get
        {
            _localizer ??= new IdentityStringLocalizer(_serviceProvider, "Boa.Identity.ResetPasswordService");
            return _localizer;
        }
    }
    private IStringLocalizer? _localizer;

    /// <inheritdoc />
    public abstract string ServiceName { get; }

    /// <inheritdoc />
    public Type DataModelType => typeof(TDataModel);

    /// <inheritdoc />
    public abstract string RequestMessage { get; }

    /// <inheritdoc />
    public abstract string ConfirmationMessage { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ResetPasswordService{}"/>.
    /// </summary>
    /// <param name="modelValidator">The <see cref="IObjectModelValidator"/> used for validating the
    /// <see cref="TDataModel"/> object.</param>
    public ResetPasswordService(IServiceProvider serviceProvider, IObjectModelValidator modelValidator)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        ModelValidator = modelValidator ?? throw new ArgumentNullException(nameof(modelValidator));
    }

    /// <inheritdoc />
    public async Task<bool> SendResetPasswordRequest(ActionContext context, object data, string prefix)
    {
        if (data is not TDataModel model)
            return false;

        _context = context;

        ModelValidator.Validate(
            context,
            validationState: null,
            prefix: prefix + ServiceName,
            model: model);

        return await ProcessAsync(model);
    }

    /// <summary>
    /// Synchronously process the reset password request after that <paramref name="data"/> are validated.
    /// The class ModelState is updated with the validation results.
    /// </summary>
    /// <param name="data">A DataModelType object with data entered by user.</param>
    /// <returns>
    /// Returns <c>true</c> if the reset password request is send, otherwise returns <c>false</c>.
    /// </returns>
    protected virtual bool Process(TDataModel data)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Asynchronously process the reset password request after that <paramref name="data"/> is validated.
    /// The class ModelState is updated with the validation results.
    /// </summary>
    /// <param name="data">A DataModelType object with data entered by user.</param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> that, when completed, returns <c>true</c> if the reset password request
    /// is send, otherwise returns <c>false</c>.
    /// </returns>
    /// <remarks>By default this calls into <see cref="Process"/>.</remarks>.
    protected virtual Task<bool> ProcessAsync(TDataModel data)
    {
        return Task.FromResult(Process(data));
    }
}
