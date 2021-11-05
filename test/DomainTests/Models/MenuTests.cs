using Domain.Models;
using FluentAssertions;
using Xunit;

namespace DomainTests.Models
{
    public class MenuTests
    {
        private readonly MenuItem _menuItem = new("MenuItem", "Menu description", 42.0, "");
        private Menu _menu;

        public MenuTests()
        {
            _menu = new Menu();
        }

        [Fact]
        public void AddMenuItemTest()
        {
            _menu = new Menu();

            _menu.MenuItems.Count.Should().Be(0);

            _menu.AddMenuItem(_menuItem);

            _menu.MenuItems.Count.Should().Be(1);
        }
    }
}