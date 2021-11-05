using Domain.Models;
using FluentAssertions;
using Xunit;

namespace DomainTests.Models
{
    public class OrderTests
    {
        [Fact]
        public void AddItemToOrderTest()
        {
            var order = new Order();
            order.Price.Should().Be(0.0);
            order.Items.Count.Should().Be(0);
        }
    }
}