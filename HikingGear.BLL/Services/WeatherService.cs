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

        public async Task<WeatherInfoDto> GetWeatherForLocationAsync(double lat, double lon, DateTime startDate, DateTime endDate)
        {
            var now = DateTime.UtcNow.Date;
            var daysUntilStart = (startDate.Date - now).TotalDays;
            var daysUntilEnd = (endDate.Date - now).TotalDays;

            if (daysUntilStart > 5 || daysUntilEnd < 0)
            {
                return new WeatherInfoDto { IsForecastAvailable = false };
            }

            var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return new WeatherInfoDto { IsForecastAvailable = false };
                }

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var list = doc.RootElement.GetProperty("list");

                var temps = new List<double>();
                bool willRain = false;
                string description = "Clear";

                foreach (var item in list.EnumerateArray())
                {
                    var dtUnix = item.GetProperty("dt").GetInt64();
                    var forecastDate = DateTimeOffset.FromUnixTimeSeconds(dtUnix).UtcDateTime.Date;

                    if (forecastDate >= startDate.Date && forecastDate <= endDate.Date)
                    {
                        var main = item.GetProperty("main");
                        temps.Add(main.GetProperty("temp").GetDouble());

                        var weatherArray = item.GetProperty("weather");
                        if (weatherArray.GetArrayLength() > 0)
                        {
                            var desc = weatherArray[0].GetProperty("description").GetString();

                            if (desc != null && (desc.Contains("rain", StringComparison.OrdinalIgnoreCase) || desc.Contains("snow", StringComparison.OrdinalIgnoreCase)))
                            {
                                willRain = true;
                                description = desc;
                            }
                        }
                    }
                }

                if (!temps.Any())
                {
                    return new WeatherInfoDto { IsForecastAvailable = false };
                }

                return new WeatherInfoDto
                {
                    TempDay = temps.Max(),
                    TempNight = temps.Min(),
                    Description = description,
                    WillRain = willRain,
                    IsForecastAvailable = true
                };
            }
            catch
            {
                return new WeatherInfoDto { IsForecastAvailable = false };
            }
        }
    }
}
