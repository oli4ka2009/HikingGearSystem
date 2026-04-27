using HikingGear.BLL.DTOs;
using HikingGear.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HikingGear.BLL.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenWeatherMap:ApiKey"]
                ?? throw new InvalidOperationException("OpenWeatherMap API Key is missing.");
        }

        public async Task<WeatherInfoDto> GetWeatherForLocationAsync(double lat, double lon)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new WeatherInfoDto { TempDay = 15, TempNight = 5, Description = "Unknown", WillRain = false };
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var main = root.GetProperty("main");
            var weatherArray = root.GetProperty("weather");
            var description = weatherArray.GetArrayLength() > 0
                ? weatherArray[0].GetProperty("description").GetString()
                : "Clear";

            return new WeatherInfoDto
            {
                TempDay = main.GetProperty("temp_max").GetDouble(),
                TempNight = main.GetProperty("temp_min").GetDouble(),
                Description = description ?? "Unknown",
                WillRain = content.Contains("rain", StringComparison.OrdinalIgnoreCase) || content.Contains("snow", StringComparison.OrdinalIgnoreCase)
            };
        }
    }
}
