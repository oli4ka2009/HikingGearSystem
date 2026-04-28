using HikingGear.BLL.DTOs;
using HikingGear.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL.Interfaces
{
    public interface IGearGenerationService
    {
        public Task<AiGearResponseDto> GenerateGearListAsync(Trip trip, WeatherInfoDto weather);
    }
}
