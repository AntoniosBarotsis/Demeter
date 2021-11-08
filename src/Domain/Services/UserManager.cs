using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Domain.Services
{
    public class UserManager: IUserManager
    {
        private readonly UserManager<User> _userManager;

        public UserManager(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public Task<User> FindByEmailAsync(string email)
        {
            return _userManager.FindByEmailAsync(email);
        }

        public Task<IdentityResult> CreateAsync(User user, string password)
        {
            return _userManager.CreateAsync(user, password);
        }
    }
}