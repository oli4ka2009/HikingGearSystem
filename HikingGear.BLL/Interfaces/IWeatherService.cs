using HikingGear.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherInfoDto> GetWeatherForLocationAsync(double lat, double lon);
    }
}
