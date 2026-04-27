using HikingGear.BLL.Services;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HikingGear.Tests
{
    public class WeatherServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;

        public WeatherServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["OpenWeatherMap:ApiKey"]).Returns("fake-test-key");
        }

        [Fact]
        public async Task GetWeatherForLocationAsync_ReturnsCorrectDto_WhenApiIsSuccessful()
        {
            // 1. Arrange
            var lat = 48.16;
            var lon = 24.50;

            var fakeJsonResponse = @"{
                ""main"": { ""temp_max"": 22.5, ""temp_min"": 10.0 },
                ""weather"": [ { ""description"": ""light rain"" } ]
            }";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeJsonResponse),
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var weatherService = new WeatherService(httpClient, _configMock.Object);

            // 2. Act
            var result = await weatherService.GetWeatherForLocationAsync(lat, lon);

            // 3. Assert
            Assert.NotNull(result);
            Assert.Equal(22.5, result.TempDay);
            Assert.Equal(10.0, result.TempNight);
            Assert.Equal("light rain", result.Description);
            Assert.True(result.WillRain);
        }

        [Fact]
        public async Task GetWeatherForLocationAsync_ReturnsFallbackValues_WhenApiFails()
        {
            // 1. Arrange
            var lat = 48.16;
            var lon = 24.50;

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var weatherService = new WeatherService(httpClient, _configMock.Object);

            // 2. Act
            var result = await weatherService.GetWeatherForLocationAsync(lat, lon);

            // 3. Assert
            Assert.NotNull(result);
            Assert.Equal(15, result.TempDay);
            Assert.Equal(5, result.TempNight);
            Assert.Equal("Unknown", result.Description);
            Assert.False(result.WillRain);
        }
    }
}
