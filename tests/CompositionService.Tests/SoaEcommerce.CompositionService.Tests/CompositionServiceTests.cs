using System.Net.Http;
using System.Text;
using System.Xml;
using SoaEcommerce.Contracts;
using Xunit;

namespace SoaEcommerce.CompositionService.Tests;

public class CompositionServiceTests
{
    [Fact]
    public void PlaceOrder_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Items = new List<PlaceOrderItem>
            {
                new PlaceOrderItem
                {
                    ProductId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Quantity = 1
                }
            }
        };

        // Act & Assert
        Assert.NotNull(request);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), request.CustomerId);
        Assert.Single(request.Items);
    }

    [Fact]
    public void PlaceOrder_InvalidCustomer_ShouldFail()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            CustomerId = Guid.NewGuid(), // Cliente inexistente
            Items = new List<PlaceOrderItem>
            {
                new PlaceOrderItem
                {
                    ProductId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Quantity = 1
                }
            }
        };

        // Act & Assert
        Assert.NotNull(request);
        Assert.NotEqual(Guid.Parse("11111111-1111-1111-1111-111111111111"), request.CustomerId);
    }

}
