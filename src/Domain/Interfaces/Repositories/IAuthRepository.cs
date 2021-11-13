using System.Threading.Tasks;
using Domain.Models.Auth;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Domain.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        Task<RefreshToken> GetRefreshTokenAsync(string refreshToken);
        EntityEntry<RefreshToken> Update(RefreshToken refreshToken);
        Task<int> SaveChangesAsync();
        ValueTask<EntityEntry<RefreshToken>> AddAsync(RefreshToken refreshToken);
    }
}