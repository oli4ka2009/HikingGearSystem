using System.Net;
using System.Text.Json;
using HikingGear.BLL.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

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
        public async Task GetWeatherForLocationAsync_ReturnsForecast_WhenApiIsSuccessfulAndDatesMatch()
        {
            // 1. Arrange
            var lat = 48.16;
            var lon = 24.50;
            var today = DateTime.UtcNow.Date;
            var startDate = today.AddDays(1);
            var endDate = today.AddDays(2);

            long dt1 = ((DateTimeOffset)startDate.AddHours(12)).ToUnixTimeSeconds();
            long dt2 = ((DateTimeOffset)endDate.AddHours(12)).ToUnixTimeSeconds();

            var fakeJsonResponse = $@"{{
                ""list"": [
                    {{
                        ""dt"": {dt1},
                        ""main"": {{ ""temp"": 10.0 }},
                        ""weather"": [ {{ ""description"": ""clear sky"" }} ]
                    }},
                    {{
                        ""dt"": {dt2},
                        ""main"": {{ ""temp"": 22.5 }},
                        ""weather"": [ {{ ""description"": ""light rain"" }} ]
                    }}
                ]
            }}";

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
            var result = await weatherService.GetWeatherForLocationAsync(lat, lon, startDate, endDate);

            // 3. Assert
            Assert.NotNull(result);
            Assert.True(result.IsForecastAvailable);
            Assert.Equal(22.5, result.TempDay);
            Assert.Equal(10.0, result.TempNight);
            Assert.Equal("light rain", result.Description);
            Assert.True(result.WillRain);
        }

        [Fact]
        public async Task GetWeatherForLocationAsync_ReturnsNotAvailable_WhenApiFails()
        {
            // 1. Arrange
            var lat = 48.16;
            var lon = 24.50;
            var startDate = DateTime.UtcNow.AddDays(1);
            var endDate = DateTime.UtcNow.AddDays(2);

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
            var result = await weatherService.GetWeatherForLocationAsync(lat, lon, startDate, endDate);

            // 3. Assert
            Assert.NotNull(result);
            Assert.False(result.IsForecastAvailable);
        }

        [Fact]
        public async Task GetWeatherForLocationAsync_ReturnsNotAvailable_WhenDatesAreOutOf5DayRange()
        {
            // 1. Arrange
            var lat = 48.16;
            var lon = 24.50;

            var startDate = DateTime.UtcNow.AddDays(10);
            var endDate = DateTime.UtcNow.AddDays(12);

            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var weatherService = new WeatherService(httpClient, _configMock.Object);

            // 2. Act
            var result = await weatherService.GetWeatherForLocationAsync(lat, lon, startDate, endDate);

            // 3. Assert
            Assert.NotNull(result);
            Assert.False(result.IsForecastAvailable);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}