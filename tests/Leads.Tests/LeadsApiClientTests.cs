using API.Clients;
using API.Models;
using Moq;
using System.Net.Http.Json;
using System.Net;
using Microsoft.Extensions.Configuration;
using Moq.Protected;

namespace Leads.Tests
{
    public class LeadsApiClientTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly LeadsApiClient _leadsApiClient;

        public LeadsApiClientTests()
        {
            var mockConfiguration = new Mock<IConfiguration>();
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(x => x.Value).Returns("https://api.example.com/leads");
            mockConfiguration.Setup(config => config.GetSection("LeadsApi:Endpoint")).Returns(mockSection.Object);

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _leadsApiClient = new LeadsApiClient(new HttpClient(_mockHttpMessageHandler.Object), mockConfiguration.Object);
        }

        [Fact]
        public async Task GetLeadsAsync_ShouldCallSendAsync_And_ReturnLeads()
        {
            // Arrange
            var leads = new List<LeadDto>
            {
                new LeadDto { Email = "lead1@test.com", Name = "Lead 1" },
                new LeadDto { Email = "lead2@test.com", Name = "Lead 2" }
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(leads)
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _leadsApiClient.GetLeadsAsync(1, 10, new DateTime(2024, 1, 1), new DateTime(2024, 1, 2));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("lead1@test.com", result.First().Email);
            Assert.Equal("lead2@test.com", result.Last().Email);

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetLeadsAsync_ShouldThrowException_WhenPageNumberIsInvalid()
        {
            // Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _leadsApiClient.GetLeadsAsync(0, 10, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));
        }

        [Fact]
        public async Task GetLeadsAsync_ShouldThrowException_WhenPageSizeIsInvalid()
        {
            // Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _leadsApiClient.GetLeadsAsync(1, 0, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));
        }

        [Fact]
        public async Task GetLeadsAsync_ShouldThrowException_WhenStartDateIsAfterEndDate()
        {
            // Arrange, Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _leadsApiClient.GetLeadsAsync(1, 10, DateTime.UtcNow.AddDays(1), DateTime.UtcNow));
        }
    }
}
