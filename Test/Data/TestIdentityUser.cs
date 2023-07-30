using Boa.Identity;
using Microsoft.AspNetCore.Identity;

namespace Test.Data
{
    public class TestIdentityUser : IdentityUser, IIdentityUser
    {
        public string? TwoFactorService { get; set; }
    }
}
