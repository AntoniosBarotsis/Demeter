using System;
using System.Threading.Tasks;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;

namespace Domain.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<Menu> AddMenuItem(Menu menu, MenuItem menuItem)
        {
            if (menu.MenuItems.Contains(menuItem))
                throw new Exception("Item already exists");
            
            if (!await _menuRepository.AddMenuItem(menu, menuItem))
                throw new Exception("Something went wrong");

            await _menuRepository.SaveChanges();
            
            menu.MenuItems.Add(menuItem);

            return menu;
        }
    }
}