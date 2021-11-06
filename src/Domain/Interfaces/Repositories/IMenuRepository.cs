using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IMenuRepository
    {
        Task<bool> AddMenuItem(Menu menu, MenuItem menuItem);
        Task<bool> SaveChanges();
    }
}