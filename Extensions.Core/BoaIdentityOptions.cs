/// <summary>
/// Represents all the options you can use to configure the boa identity extension.
/// </summary>
namespace Boa.Identity
{
    public class BoaIdentityOptions
    {
        /// <summary>
        /// Gets or sets if users are forced to use the two factor authentication.
        /// </summary>
        public bool ForceTwoFactorAuthentication { get; set; } = false;
    }
}
