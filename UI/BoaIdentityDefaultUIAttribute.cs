namespace Boa.Identity.UI;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class BoaIdentityDefaultUIAttribute : Attribute
{
    public BoaIdentityDefaultUIAttribute(Type implementationTemplate)
    {
        Template = implementationTemplate;
    }

    public Type Template { get; }
}
