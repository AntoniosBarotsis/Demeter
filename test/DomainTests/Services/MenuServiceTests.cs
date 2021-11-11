using System.Threading.Tasks;
using AutoFixture;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Domain.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DomainTests.Services
{
    public class MenuServiceTests
    {
        private const double Price1 = 42.0;
        private readonly IFixture _fixture = new Fixture();
        private readonly Menu _menu;
        private readonly MenuItem _menuItem1;

        private readonly IMenuRepository _menuRepository = Substitute.For<IMenuRepository>();
        private readonly MenuService _sut;

        public MenuServiceTests()
        {
            _sut = new MenuService(_menuRepository);

            _menu = _fixture
                .Build<Menu>()
                .Create();

            _menuItem1 = _fixture
                .Build<MenuItem>()
                .With(i => i.Price, Price1)
                .Create();
        }

        [Fact]
        public async Task AddMenuItemTest()
        {
            _menuRepository.AddMenuItem(_menu, _menuItem1).Returns(Task.FromResult(true));

            var res = await _sut.AddMenuItem(_menu, _menuItem1);

            res.Should().NotBeNull();
            res.MenuItems.Count.Should().Be(1);
        }

        [Fact]
        public async Task AddDuplicateItemToMenu()
        {
            _menuRepository.AddMenuItem(_menu, _menuItem1).Returns(Task.FromResult(true));

            var res = await _sut.AddMenuItem(_menu, _menuItem1);

            res.Should().NotBeNull();
            res.MenuItems.Count.Should().Be(1);

            var exception = Record
                .ExceptionAsync(async () => await _sut.AddMenuItem(_menu, _menuItem1));

            exception.Should().NotBeNull();
            exception.Result.Should().NotBeNull();
            exception.Result?.Message.Should().Be("Item already exists");
        }

        [Fact]
        public void AddItemToMenuQueryException()
        {
            _menuRepository.AddMenuItem(_menu, _menuItem1).Returns(Task.FromResult(false));
            var exception = Record
                .ExceptionAsync(async () => await _sut.AddMenuItem(_menu, _menuItem1));

            exception.Should().NotBeNull();
            exception.Result.Should().NotBeNull();
            exception.Result?.Message.Should().Be("Something went wrong");
        }
    }
}