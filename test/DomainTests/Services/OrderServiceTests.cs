using System;
using AutoFixture;
using Domain.Interfaces;
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

        private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _sut = new OrderService(_orderRepository);
        }

        [Fact]
        public void AddItemToOrderTest()
        {
            var menuItem1 = _fixture
                .Build<MenuItem>()
                .With(i => i.Price, Price1)
                .Create();

            var menuItem2 = _fixture
                .Build<MenuItem>()
                .With(i => i.Price, Price2)
                .Create();

            _orderRepository.UpdateOrder(Arg.Any<Order>()).Returns(true);

            var order = _sut.AddItemToOrder(menuItem1);

            order.Price.Should().Be(Price1);

            _sut.AddItemToOrder(order, menuItem2);

            order.Price.Should().Be(Price1 + Price2);

            _sut.AddItemToOrder(order, menuItem2, 2);

            order.Price.Should().Be(Price1 + Price2 * 3);
        }

        [Fact]
        public void AddItemToOrderWithBadCount()
        {
            var menuItem1 = _fixture
                .Create<MenuItem>();

            Action act = () => _sut.AddItemToOrder(menuItem1, 0);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("count cannot be less than 0. Was 0");
        }

        [Fact]
        public void AddItemToOrderWithSaveChangesFalse()
        {
            var menuItem1 = _fixture
                .Create<MenuItem>();

            var order = _fixture
                .Create<Order>();

            _orderRepository.UpdateOrder(order).Returns(false);

            Action act = () => _sut.AddItemToOrder(order, menuItem1);

            act.Should()
                .Throw<Exception>()
                .WithMessage("Something went wrong");
        }
    }
}