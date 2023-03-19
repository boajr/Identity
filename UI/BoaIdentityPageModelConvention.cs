using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Boa.Identity.UI;

internal sealed class BoaIdentityPageModelConvention<TUser> : IPageApplicationModelConvention where TUser : class
{
    public void Apply(PageApplicationModel model)
    {
        var defaultUIAttribute = model.ModelType?.GetCustomAttribute<BoaIdentityDefaultUIAttribute>();
        if (defaultUIAttribute == null)
        {
            return;
        }

        ValidateTemplate(defaultUIAttribute.Template);
        var templateInstance = defaultUIAttribute.Template.MakeGenericType(typeof(TUser));
        model.ModelType = templateInstance.GetTypeInfo();
    }

    private static void ValidateTemplate(Type template)
    {
        if (template.IsAbstract || !template.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException("Implementation type can't be abstract or non generic.");
        }
        var genericArguments = template.GetGenericArguments();
        if (genericArguments.Length != 1)
        {
            throw new InvalidOperationException("Implementation type contains wrong generic arity.");
        }
    }
}
