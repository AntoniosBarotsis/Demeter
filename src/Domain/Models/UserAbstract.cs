using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    /// <summary>
    ///     The abstract user class. Inherits from IdentityUser which already has a lot of the information we would need
    ///     such as Username, Email, Password etc
    /// </summary>
    public abstract class UserAbstract : IdentityUser
    {
        public UserType UserType { get; set; }
    }

    /// <summary>
    /// Different user types. Owner is for restaurant owner while user is a normal consumer.
    /// </summary>
    public enum UserType
    {
        Owner,
        User
    }
}