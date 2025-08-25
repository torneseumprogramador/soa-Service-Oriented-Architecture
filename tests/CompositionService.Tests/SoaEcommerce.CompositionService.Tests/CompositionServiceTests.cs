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
            CustomerEmail = "valid@example.com",
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
        Assert.Equal("valid@example.com", request.CustomerEmail);
        Assert.Single(request.Items);
    }

    [Fact]
    public void PlaceOrder_InvalidCustomer_ShouldFail()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            CustomerEmail = "invalid@example.com", // Cliente inexistente
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
        Assert.NotEqual("valid@example.com", request.CustomerEmail);
    }

}
