using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL.DTOs
{
    public class WeatherInfoDto
    {
        public double TempDay { get; set; }
        public double TempNight { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool WillRain { get; set; }
    }
}
