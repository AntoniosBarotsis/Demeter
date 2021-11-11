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
    public class OrderServiceTests
    {
        private const double Price1 = 42.0;
        private const double Price2 = 2.0;
        private readonly IFixture _fixture = new Fixture();
        private readonly MenuItem _menuItem1;
        private readonly MenuItem _menuItem2;
        private readonly Order _order;

        private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _sut = new OrderService(_orderRepository);

            _menuItem1 = _fixture
                .Build<MenuItem>()
                .With(i => i.Price, -1)
                .Create();

            _menuItem2 = _fixture
                .Build<MenuItem>()
                .With(i => i.Price, -1)
                .Create();

            _order = _fixture
                .Create<Order>();
        }

        [Fact]
        public async Task AddItemToOrderTest()
        {
            _menuItem1.Price = Price1;
            _menuItem2.Price = Price2;

            _orderRepository.UpdateOrder(Arg.Any<Order>()).Returns(Task.FromResult(true));

            var order = await _sut.AddItemToOrder(_menuItem1);

            order.Price.Should().Be(Price1);
            await _orderRepository.Received(1).UpdateOrder(order);
            await _orderRepository.Received(1).SaveChanges();

            await _sut.AddItemToOrder(order, _menuItem2);

            order.Price.Should().Be(Price1 + Price2);
            await _orderRepository.Received(2).UpdateOrder(order);
            await _orderRepository.Received(2).SaveChanges();

            await _sut.AddItemToOrder(order, _menuItem2, 2);

            order.Price.Should().Be(Price1 + Price2 * 3);
            await _orderRepository.Received(3).UpdateOrder(order);
            await _orderRepository.Received(3).SaveChanges();
        }

        [Fact]
        public void AddItemToOrderWithBadCount()
        {
            var task = Task.Run(() => _sut.AddItemToOrder(_menuItem1, 0));

            var exception = Record.ExceptionAsync(async () => await task);

            exception.Should().NotBeNull();
            exception.Result.Should().NotBeNull();
            exception.Result?.Message.Should().Be("count cannot be less than 0. Was 0");
        }

        [Fact]
        public void AddItemToOrderWithUpdateOrderFalse()
        {
            _orderRepository.UpdateOrder(_order).Returns(Task.FromResult(false));

            var task = Task.Run(() => _sut.AddItemToOrder(_order, _menuItem1));
            var exception = Record.ExceptionAsync(async () => await task);

            exception.Should().NotBeNull();
            exception.Result.Should().NotBeNull();
            exception.Result?.Message.Should().Be("Something went wrong");
        }
    }
}