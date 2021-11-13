using System.Threading.Tasks;
using Domain.Interfaces.Repositories;
using Domain.Models.Auth;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories
{
    public class AuthRepository: IAuthRepository
    {
        private readonly DemeterDbContext _context;

        public AuthRepository(DemeterDbContext context)
        {
            _context = context;
        }

        public Task<RefreshToken> GetRefreshTokenAsync(string refreshToken)
        {
            return _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
        }

        public EntityEntry<RefreshToken> Update(RefreshToken refreshToken)
        {
            return _context.RefreshTokens.Update(refreshToken);
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public ValueTask<EntityEntry<RefreshToken>> AddAsync(RefreshToken refreshToken)
        {
            return _context.RefreshTokens.AddAsync(refreshToken);
        }
    }
}