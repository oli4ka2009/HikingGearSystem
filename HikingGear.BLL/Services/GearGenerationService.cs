using HikingGear.BLL.DTOs;
using HikingGear.BLL.Interfaces;
using HikingGear.DAL.Repositories;
using HikingGear.Models.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HikingGear.BLL.Services
{
    public class GearGenerationService : IGearGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ITripRepository _tripRepository;
        private readonly IWeatherService _weatherService;
        private readonly ILogger<GearGenerationService> _logger;
        private readonly IGearItemRepository _gearItemRepository;
        private readonly ICategoryRepository _categoryRepository;

        public GearGenerationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ITripRepository tripRepository,
            IWeatherService weatherService,
            ILogger<GearGenerationService> logger,
            IGearItemRepository gearItemRepository,
            ICategoryRepository categoryRepository)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API Key is missing.");
            _tripRepository = tripRepository;
            _weatherService = weatherService;
            _logger = logger;
            _gearItemRepository = gearItemRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<TripGearResponseDto> GenerateGearListAsync(int tripId)
        {
            var trip = await GetValidTripAsync(tripId);
            /*
            // ТИМЧАСОВА ЗАГЛУШКА (MOCK) ПОКИ GOOGLE ЛЕЖИТЬ
            _logger.LogInformation("Використовуємо Mock-дані, бо Gemini API видає 503...");
            await Task.Delay(2000);

            var mockDto = new AiGearResponseDto
            {
                Categories = new List<AiCategoryDto>
        {
            new AiCategoryDto
            {
                CategoryName = "Одяг (Mock)",
                Items = new List<AiGearItemDto>
                {
                    new AiGearItemDto { Name = "Мембранна куртка", WeightInGrams = 450, Quantity = 1, IsGroupGear = false, IsWearable = true },
                    new AiGearItemDto { Name = "Запасні шкарпетки", WeightInGrams = 50, Quantity = 2, IsGroupGear = false, IsWearable = false }
                }
            },
            new AiCategoryDto
            {
                CategoryName = "Групове (Mock)",
                Items = new List<AiGearItemDto>
                {
                    new AiGearItemDto { Name = "Намет 3-місний", WeightInGrams = 2500, Quantity = 1, IsGroupGear = true, IsWearable = false }
                }
            }
        }
            };

            // ✅ Зберігаємо в базу (новий метод видаляє старі категорії цього походу і створює нові)
            await SaveGeneratedGearAsync(tripId, mockDto);

            // ✅ Дістаємо збережені категорії з речами через CategoryRepository
            var categories = await _categoryRepository.GetByTripIdAsync(tripId); // Include(GearItems) всередині

            return new TripGearResponseDto
            {
                Categories = categories.Select(c => new TripCategoryDto
                {
                    Id = c.Id,
                    CategoryName = c.Name,
                    Items = c.GearItems.Select(i => new TripGearItemDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        WeightInGrams = i.WeightInGrams,
                        Quantity = i.Quantity,
                        IsGroupGear = i.IsGroupGear,
                        IsWearable = i.IsWearable,
                        IsPacked = i.IsPacked
                    }).ToList()
                }).ToList()
            }; */

            var weather = await FetchWeatherAsync(trip);
            _logger.LogInformation("Отримано погоду для походу ID {TripId}: {WeatherData}",
                tripId, JsonSerializer.Serialize(weather));
            var prompt = BuildPrompt(trip, weather);
            var aiJsonResponse = await SendGeminiRequestAsync(prompt);
            var generatedDto = ParseGeminiResponse(aiJsonResponse);
            await SaveGeneratedGearAsync(tripId, generatedDto);
            var categories = await _categoryRepository.GetByTripIdAsync(tripId);
            return new TripGearResponseDto
            {
                Categories = categories.Select(c => new TripCategoryDto
                {
                    Id = c.Id,
                    CategoryName = c.Name,
                    Items = c.GearItems.Select(i => new TripGearItemDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        WeightInGrams = i.WeightInGrams,
                        Quantity = i.Quantity,
                        IsGroupGear = i.IsGroupGear,
                        IsWearable = i.IsWearable,
                        IsPacked = i.IsPacked
                    }).ToList()
                }).ToList()
            };
        }

        private async Task SaveGeneratedGearAsync(int tripId, AiGearResponseDto dto)
        {
            var existingCategories = await _categoryRepository.GetByTripIdAsync(tripId);
            foreach (var cat in existingCategories)
                await _categoryRepository.DeleteAsync(cat);
            await _categoryRepository.SaveChangesAsync();

            if (dto.Categories == null || !dto.Categories.Any()) return;

            foreach (var categoryDto in dto.Categories)
            {
                var category = new GearCategory
                {
                    Name = categoryDto.CategoryName,
                    TripId = tripId
                };

                await _categoryRepository.AddAsync(category);
                await _categoryRepository.SaveChangesAsync();

                var items = categoryDto.Items.Select(itemDto => new GearItem
                {
                    CategoryId = category.Id,
                    Name = itemDto.Name,
                    WeightInGrams = itemDto.WeightInGrams,
                    Quantity = itemDto.Quantity,
                    IsGroupGear = itemDto.IsGroupGear,
                    IsWearable = itemDto.IsWearable,
                    IsPacked = false
                }).ToList();

                await _gearItemRepository.AddItemsAsync(items);
            }
        }

        private async Task<Trip> GetValidTripAsync(int tripId)
        {
            var trip = await _tripRepository.GetTripByIdAsync(tripId);
            if (trip == null)
            {
                throw new KeyNotFoundException($"Похід з ID {tripId} не знайдено.");
            }
            return trip;
        }

        private async Task<WeatherInfoDto> FetchWeatherAsync(Trip trip)
        {
            return await _weatherService.GetWeatherForLocationAsync(
                trip.Latitude,
                trip.Longitude,
                trip.StartDate,
                trip.EndDate);
        }

        private async Task<string> SendGeminiRequestAsync(string prompt)
        {
            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { response_mime_type = "application/json" }
            };

            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private AiGearResponseDto ParseGeminiResponse(string jsonResponse)
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var textResponse = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(textResponse))
            {
                return new AiGearResponseDto();
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<AiGearResponseDto>(textResponse, options);

            return result ?? new AiGearResponseDto();
        }

        private string BuildPrompt(Trip trip, WeatherInfoDto weather)
        {
            var durationDays = Math.Max(1, (trip.EndDate.Date - trip.StartDate.Date).TotalDays);

            var weatherContext = weather.IsForecastAvailable
                ? $"Температура вдень: {weather.TempDay}°C, вночі: {weather.TempNight}°C. Опис: {weather.Description}. Опади: {(weather.WillRain ? "Очікуються" : "Не очікуються")}."
                : "Точний прогноз погоди недоступний. Базуй рекомендації на типових кліматичних умовах для цього сезону та локації.";

            var accommodationContext = trip.AccommodationFormat switch
            {
                SleepFormat.Tent => "Автономна ночівля в наметі (потрібне повне бівуачне спорядження)",
                SleepFormat.Shelter => "Ночівля у туристичному притулку або колибі (намет не потрібен, але потрібен спальник)",
                _ => "Без ночівлі (одноденний похід)"
            };

            return $@"
Ти — експерт з альпінізму та гірського туризму. Твоє завдання - згенерувати детальний, категоризований список спорядження для туристичного походу.

Деталі походу:
- Локація: {trip.LocationName}
- Тривалість: {durationDays} днів ({trip.StartDate:dd.MM.yyyy} - {trip.EndDate:dd.MM.yyyy})
- Розмір групи: {trip.GroupSize} осіб
- Формат ночівлі: {accommodationContext}
- Погодні умови: {weatherContext}

Вимоги до списку:
1. Застосуй концепцію 'багатошаровості' для одягу (базовий, утеплюючий, захисний шари).
2. Розділи спорядження на особисте та групове. Вкажи поле isGroupGear як true або false.
3. Оціни приблизну масу кожної речі в грамах (поле weightInGrams) для ОДНІЄЇ одиниці (quantity = 1).
4. Розрахуй кількість (quantity) за такими суворими правилами:
   - Для особистого спорядження (isGroupGear = false) вказуй кількість ТІЛЬКИ ДЛЯ ОДНІЄЇ ЛЮДИНИ (наприклад, 1 спальник, 2 пари шкарпеток).
   - Для групового спорядження (isGroupGear = true) розрахуй загальну кількість, необхідну для всієї групи з {trip.GroupSize} осіб (наприклад, 1 тримісний намет).
5. Визнач, чи є річ 'носимою' (isWearable: true). Це одяг на тілі (базовий шар), взуття і т.д..
6. КРИТИЧНО: Якщо річ має запасні одиниці (наприклад, 3 пари шкарпеток), розділи її на два об'єкти: 
   - 1 пара з isWearable: true (на собі).
   - 2 пари з isWearable: false (у рюкзаку).

Ти ПОВИНЕН повернути результат ВИКЛЮЧНО у форматі валідного JSON, який точно відповідає такій структурі (без маркдаун-розмітки, лише сирий JSON):
{{
    ""categories"": [
        {{
            ""categoryName"": ""назва категорії (наприклад: Одяг, Кухня, Сон, Навігація, Аптечка)"",
            ""items"": [
                {{
                    ""name"": ""Назва речі (наприклад: Мембранна куртка)"",
                    ""weightInGrams"": 450.0,
                    ""quantity"": 1,
                    ""isGroupGear"": false
                    ""isWearable"": true
                }}
            ]
        }}
    ]
}}
";
        }
    }
}
