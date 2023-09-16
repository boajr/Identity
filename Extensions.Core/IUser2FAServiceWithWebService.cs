using Microsoft.AspNetCore.Identity;

namespace Boa.Identity;

public interface IUser2FAServiceWithWebService<TUser> : IUser2FAService<TUser>
    where TUser : class
{

}
