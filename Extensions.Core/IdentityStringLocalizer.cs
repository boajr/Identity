using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;

namespace Boa.Identity
{
    public class IdentityStringLocalizer : IStringLocalizer
    {
        private readonly IStringLocalizer? _localizer;

        public LocalizedString this[string name]
        {
            get
            {
                ArgumentNullException.ThrowIfNull(name);

                if (_localizer != null)
                {
                    return _localizer[name];
                }
                return new LocalizedString(name, name, false);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                ArgumentNullException.ThrowIfNull(name);

                if (_localizer != null)
                {
                    return _localizer[name, arguments];
                }
                return new LocalizedString(name, string.Format(CultureInfo.CurrentCulture, name, arguments), false);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _localizer != null ? _localizer.GetAllStrings(includeParentCultures) : [];
        }

        public IdentityStringLocalizer(IServiceProvider serviceProvider, string baseName)
        {
            string? assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (assemblyName != null)
            {
                var pos = baseName.IndexOf('`');
                _localizer = serviceProvider.GetService<IStringLocalizerFactory>()?.Create(pos >= 0 ? baseName.Remove(pos) : baseName, assemblyName);
            }
        }
    }
}
