using HikingGear.BLL.DTOs;
using HikingGear.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL.Interfaces
{
    public interface IGearItemService
    {
        Task<TripGearResponseDto> GetGearForTripAsync(int tripId, int userId);

        Task<GearItem> AddCustomGearItemAsync(int userId, GearItemCreateDto dto);
        Task UpdateGearItemAsync(int userId, int itemId, GearItemUpdateDto dto);
        Task DeleteGearItemAsync(int userId, int itemId);

        Task UpdatePackedStatusAsync(int userId, int itemId, bool isPacked);

        Task<PackingProgressDto> GetPackingProgressAsync(int userId, int tripId);
    }
}
