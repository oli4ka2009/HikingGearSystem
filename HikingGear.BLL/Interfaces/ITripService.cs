using HikingGear.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL.Interfaces
{
    public interface ITripService
    {
        Task<TripResponseDto> CreateTripAsync(int userId, TripCreateDto dto);
        Task<IEnumerable<TripResponseDto>> GetUserTripsAsync(int userId);
        Task<TripResponseDto?> GetTripByIdAsync(int id);
        Task DeleteTripAsync(int userId, int tripId);
        Task<bool> UpdateTripAsync(int tripId, int userId, TripUpdateDto dto);
    }
}
