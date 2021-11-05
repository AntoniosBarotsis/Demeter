using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    /// <summary>
    ///     The abstract user class. Inherits from IdentityUser which already has a lot of the information we would need
    ///     such as Username, Email, Password etc
    /// </summary>
    public abstract class UserAbstract : IdentityUser
    {
    }
}