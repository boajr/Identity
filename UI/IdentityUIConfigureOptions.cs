using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Boa.Identity.UI
{
    internal sealed class IdentityUIConfigureOptions<TUser> :
    IPostConfigureOptions<RazorPagesOptions> where TUser : class
    {
        private const string IdentityUIDefaultAreaName = "Identity";

        public IdentityUIConfigureOptions(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public IWebHostEnvironment Environment { get; }

        public void PostConfigure(string? name, RazorPagesOptions options)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));

            var convention = new IdentityPageModelConvention<TUser>();
            options.Conventions.AddAreaFolderApplicationModelConvention(
                IdentityUIDefaultAreaName,
                "/",
                convention.Apply);
        }
    }
}
