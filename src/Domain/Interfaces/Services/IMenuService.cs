using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces.Services
{
    public interface IMenuService
    {
        Task<Menu> AddMenuItem(Menu menu, MenuItem menuItem);
    }
}