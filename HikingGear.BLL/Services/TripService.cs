using HikingGear.BLL.DTOs;
using HikingGear.BLL.Interfaces;
using HikingGear.DAL.Repositories;
using HikingGear.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL.Services
{
    public class TripService : ITripService
    {
        private readonly ITripRepository _tripRepository;

        public TripService(ITripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }

        public async Task<TripResponseDto> CreateTripAsync(int userId, TripCreateDto dto)
        {
            var trip = new Trip
            {
                UserId = userId,
                Title = dto.Title,
                LocationName = dto.LocationName,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                GroupSize = dto.GroupSize,
                AccommodationFormat = dto.AccommodationFormat
            };

            await _tripRepository.AddTripAsync(trip);

            return MapToResponse(trip);
        }

        public async Task<IEnumerable<TripResponseDto>> GetUserTripsAsync(int userId)
        {
            var trips = await _tripRepository.GetUserTripsAsync(userId);
            return trips.Select(MapToResponse);
        }

        public async Task<TripResponseDto?> GetTripByIdAsync(int id)
        {
            var trip = await _tripRepository.GetTripByIdAsync(id);
            return trip == null ? null : MapToResponse(trip);
        }

        private static TripResponseDto MapToResponse(Trip trip)
        {
            return new TripResponseDto
            {
                Id = trip.Id,
                UserId = trip.UserId,
                Title = trip.Title,
                LocationName = trip.LocationName,
                Latitude = trip.Latitude,
                Longitude = trip.Longitude,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                GroupSize = trip.GroupSize,
                AccommodationFormat = trip.AccommodationFormat
            };
        }

        public async Task DeleteTripAsync(int userId, int tripId)
        {
            var trip = await _tripRepository.GetTripByIdAsync(tripId);

            if (trip == null)
                throw new KeyNotFoundException("Похід не знайдено.");

            if (trip.UserId != userId)
                throw new UnauthorizedAccessException("Ви не маєте доступу до видалення цього походу.");

            await _tripRepository.DeleteTripAsync(trip);
        }

        public async Task<bool> UpdateTripAsync(int tripId, int userId, TripUpdateDto dto)
        {
            var trip = await _tripRepository.GetTripByIdAsync(tripId);

            if (trip == null || trip.UserId != userId)
                return false;

            trip.Title = dto.Title;
            trip.LocationName = dto.LocationName;
            trip.Latitude = dto.Latitude;
            trip.Longitude = dto.Longitude;
            trip.StartDate = dto.StartDate;
            trip.EndDate = dto.EndDate;
            trip.GroupSize = dto.GroupSize;
            trip.AccommodationFormat = dto.AccommodationFormat;

            await _tripRepository.UpdateTripAsync(trip);

            return true;
        }
    }
}
