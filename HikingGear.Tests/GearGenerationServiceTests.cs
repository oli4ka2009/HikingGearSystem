using HikingGear.BLL.DTOs;
using HikingGear.BLL.Interfaces;
using HikingGear.BLL.Services;
using HikingGear.DAL.Repositories;
using HikingGear.Models.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace HikingGear.Tests
{
    public class GearGenerationServiceTests
    {
        private readonly Mock<ITripRepository> _tripRepoMock;
        private readonly Mock<IWeatherService> _weatherServiceMock;
        private readonly Mock<IGearItemRepository> _gearItemRepoMock;
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<ILogger<GearGenerationService>> _loggerMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public GearGenerationServiceTests()
        {
            _tripRepoMock = new Mock<ITripRepository>();
            _weatherServiceMock = new Mock<IWeatherService>();
            _gearItemRepoMock = new Mock<IGearItemRepository>();
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<GearGenerationService>>();
            _configMock = new Mock<IConfiguration>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _configMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        }

        private GearGenerationService CreateService()
        {
            return new GearGenerationService(
                _httpClient,
                _configMock.Object,
                _tripRepoMock.Object,
                _weatherServiceMock.Object,
                _loggerMock.Object,
                _gearItemRepoMock.Object,
                _categoryRepoMock.Object);
        }

        [Fact]
        public void Constructor_MissingApiKey_ThrowsInvalidOperationException()
        {
            // Arrange
            _configMock.Setup(c => c["Gemini:ApiKey"]).Returns((string?)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => CreateService());
        }

        [Fact]
        public async Task GenerateGearListAsync_TripNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _tripRepoMock.Setup(r => r.GetTripByIdAsync(It.IsAny<int>())).ReturnsAsync((Trip?)null);
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GenerateGearListAsync(1));
        }

        [Fact]
        public async Task GenerateGearListAsync_GeminiApiReturnsError_ThrowsHttpRequestException()
        {
            // Arrange
            var trip = new Trip { Id = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(2) };
            _tripRepoMock.Setup(r => r.GetTripByIdAsync(1)).ReturnsAsync(trip);
            _weatherServiceMock.Setup(w => w.GetWeatherForLocationAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new WeatherInfoDto());

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GenerateGearListAsync(1));
        }

        [Fact]
        public async Task GenerateGearListAsync_GeminiReturnsMalformedJson_ThrowsJsonException()
        {
            // Arrange
            var trip = new Trip { Id = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(2) };
            _tripRepoMock.Setup(r => r.GetTripByIdAsync(1)).ReturnsAsync(trip);
            _weatherServiceMock.Setup(w => w.GetWeatherForLocationAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new WeatherInfoDto());

            var geminiResponse = new
            {
                candidates = new[]
                {
                    new
                    {
                        content = new
                        {
                            parts = new[]
                            {
                                new { text = "invalid json content" }
                            }
                        }
                    }
                }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(geminiResponse))
                });

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => service.GenerateGearListAsync(1));
        }

        [Fact]
        public async Task GenerateGearListAsync_GeminiReturnsEmptyCategories_ReturnsEmptyResponseDto()
        {
            // Arrange
            var trip = new Trip { Id = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(2) };
            _tripRepoMock.Setup(r => r.GetTripByIdAsync(1)).ReturnsAsync(trip);
            _weatherServiceMock.Setup(w => w.GetWeatherForLocationAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new WeatherInfoDto());

            var geminiResponse = new
            {
                candidates = new[]
                {
                    new
                    {
                        content = new
                        {
                            parts = new[]
                            {
                                new { text = "{\"categories\": []}" }
                            }
                        }
                    }
                }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(geminiResponse))
                });

            _categoryRepoMock.Setup(r => r.GetByTripIdAsync(1)).ReturnsAsync(new List<GearCategory>());

            var service = CreateService();

            // Act
            var result = await service.GenerateGearListAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Categories);
        }

        [Fact]
        public async Task GenerateGearListAsync_SuccessfulGeneration_SavesAndReturnsData()
        {
            // Arrange
            var trip = new Trip 
            { 
                Id = 1, 
                StartDate = DateTime.Now, 
                EndDate = DateTime.Now.AddDays(1),
                LocationName = "Carpathians",
                GroupSize = 2,
                AccommodationFormat = SleepFormat.Tent
            };
            _tripRepoMock.Setup(r => r.GetTripByIdAsync(1)).ReturnsAsync(trip);
            _weatherServiceMock.Setup(w => w.GetWeatherForLocationAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new WeatherInfoDto { TempDay = 15, Description = "Cloudy", IsForecastAvailable = true });

            var aiGearResponse = new AiGearResponseDto
            {
                Categories = new List<AiCategoryDto>
                {
                    new AiCategoryDto
                    {
                        CategoryName = "Essentials",
                        Items = new List<AiGearItemDto>
                        {
                            new AiGearItemDto { Name = "Backpack", WeightInGrams = 1500, Quantity = 1, IsWearable = false, IsGroupGear = false }
                        }
                    }
                }
            };

            var geminiResponse = new
            {
                candidates = new[]
                {
                    new
                    {
                        content = new
                        {
                            parts = new[]
                            {
                                new { text = JsonSerializer.Serialize(aiGearResponse) }
                            }
                        }
                    }
                }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(geminiResponse))
                });

            // For SaveGeneratedGearAsync
            _categoryRepoMock.Setup(r => r.GetByTripIdAsync(1)).ReturnsAsync(new List<GearCategory>());
            
            // Mock category after save
            var savedCategory = new GearCategory { Id = 5, Name = "Essentials", TripId = 1 };
            savedCategory.GearItems = new List<GearItem>
            {
                new GearItem { Id = 50, Name = "Backpack", WeightInGrams = 1500, Quantity = 1, CategoryId = 5 }
            };

            // Second call to GetByTripIdAsync (after saving)
            _categoryRepoMock.SetupSequence(r => r.GetByTripIdAsync(1))
                .ReturnsAsync(new List<GearCategory>()) // First call in SaveGeneratedGearAsync
                .ReturnsAsync(new List<GearCategory> { savedCategory }); // Second call at the end of GenerateGearListAsync

            var service = CreateService();

            // Act
            var result = await service.GenerateGearListAsync(1);

            // Assert
            Assert.Single(result.Categories);
            Assert.Equal("Essentials", result.Categories[0].CategoryName);
            Assert.Single(result.Categories[0].Items);
            Assert.Equal("Backpack", result.Categories[0].Items[0].Name);

            _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<GearCategory>()), Times.Once);
            _gearItemRepoMock.Verify(r => r.AddItemsAsync(It.IsAny<IEnumerable<GearItem>>()), Times.Once);
            _categoryRepoMock.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GenerateGearListAsync_BoundaryDates_UsesMinimumOneDayDuration()
        {
            // Arrange
            var trip = new Trip { Id = 1, StartDate = DateTime.Today, EndDate = DateTime.Today };
            _tripRepoMock.Setup(r => r.GetTripByIdAsync(1)).ReturnsAsync(trip);
            _weatherServiceMock.Setup(w => w.GetWeatherForLocationAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new WeatherInfoDto());

            string? capturedPrompt = null;
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>(async (req, ct) => {
                    var content = await req.Content!.ReadAsStringAsync();
                    capturedPrompt = content;
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"{\\\"categories\\\":[]}\"}]}}]}")
                });

            _categoryRepoMock.Setup(r => r.GetByTripIdAsync(1)).ReturnsAsync(new List<GearCategory>());

            var service = CreateService();

            // Act
            await service.GenerateGearListAsync(1);

            // Assert
            Assert.NotNull(capturedPrompt);
            using var doc = JsonDocument.Parse(capturedPrompt);
            var text = doc.RootElement
                .GetProperty("contents")[0]
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            Assert.Contains("Тривалість: 1 днів", text);
        }
    }
}
