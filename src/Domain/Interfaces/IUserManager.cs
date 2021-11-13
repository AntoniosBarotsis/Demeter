using System.Threading.Tasks;
using Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Domain.Interfaces
{
    public interface IUserManager
    {
        Task<User> FindByEmailAsync(string email);
        Task<IdentityResult> CreateAsync(User user, string password);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<User> FindByIdAsync(string userId);
    }
}