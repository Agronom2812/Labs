using RecordManagement.Core.API;
using Moq;
using Xunit;
using System.Net;
using System.Text.Json;
using Moq.Protected;

namespace RecordManagement.Tests
{
    public sealed class QuoteServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;
        private readonly QuoteService _service;

        public QuoteServiceTests()
        {
            _mockHttpHandler = new Mock<HttpMessageHandler>();
            HttpClient httpClient = new(_mockHttpHandler.Object);
            _service = new QuoteService(httpClient);
        }

        [Fact]
        public async Task GetRandomQuoteAsync_FailedApiCall_ReturnsFallbackQuote()
        {
            // Given
            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException());

            // When
            var result = await _service.GetRandomQuoteAsync();

            // Then
            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Author);
        }
    }
}
