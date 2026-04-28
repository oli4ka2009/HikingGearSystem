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
    public class GearItemService : IGearItemService
    {
        private readonly IGearItemRepository _gearItemRepository;
        private readonly ITripRepository _tripRepository;

        public GearItemService(IGearItemRepository gearItemRepository, ITripRepository tripRepository)
        {
            _gearItemRepository = gearItemRepository;
            _tripRepository = tripRepository;
        }

        private async Task EnsureUserOwnsTripAsync(int userId, int tripId)
        {
            var trip = await _tripRepository.GetTripByIdAsync(tripId);
            if (trip == null)
                throw new KeyNotFoundException("Похід не знайдено.");

            if (trip.UserId != userId)
                throw new UnauthorizedAccessException("Ви не маєте доступу до керування спорядженням цього походу.");
        }

        public async Task<IEnumerable<GearItem>> GetGearForTripAsync(int tripId, int userId)
        {
            await EnsureUserOwnsTripAsync(userId, tripId);

            return await _gearItemRepository.GetItemsByTripIdAsync(tripId);
        }

        public async Task<GearItem> AddCustomGearItemAsync(int userId, GearItemCreateDto dto)
        {
            await EnsureUserOwnsTripAsync(userId, dto.TripId);

            int categoryId = await _gearItemRepository.GetOrCreateCategoryIdAsync(dto.CategoryName);

            var newItem = new GearItem
            {
                TripId = dto.TripId,
                CategoryId = categoryId,
                Name = dto.Name,
                WeightInGrams = dto.WeightInGrams,
                Quantity = dto.Quantity,
                IsGroupGear = dto.IsGroupGear,
                IsPacked = false
            };

            await _gearItemRepository.AddItemsAsync(new List<GearItem> { newItem });
            return newItem;
        }

        public async Task UpdateGearItemAsync(int userId, int itemId, GearItemUpdateDto dto)
        {
            var item = await _gearItemRepository.GetItemByIdAsync(itemId);
            if (item == null) throw new KeyNotFoundException("Річ не знайдено.");

            await EnsureUserOwnsTripAsync(userId, item.TripId);

            item.Name = dto.Name;
            item.WeightInGrams = dto.WeightInGrams;
            item.Quantity = dto.Quantity;
            item.IsGroupGear = dto.IsGroupGear;

            await _gearItemRepository.UpdateItemAsync(item);
        }

        public async Task DeleteGearItemAsync(int userId, int itemId)
        {
            var item = await _gearItemRepository.GetItemByIdAsync(itemId);
            if (item == null) throw new KeyNotFoundException("Річ не знайдено.");

            await EnsureUserOwnsTripAsync(userId, item.TripId);

            await _gearItemRepository.DeleteItemAsync(item);
        }

        public async Task UpdatePackedStatusAsync(int userId, int itemId, bool isPacked)
        {
            var item = await _gearItemRepository.GetItemByIdAsync(itemId);
            if (item == null) throw new KeyNotFoundException("Річ не знайдено.");

            await EnsureUserOwnsTripAsync(userId, item.TripId);

            item.IsPacked = isPacked;
            await _gearItemRepository.UpdateItemAsync(item);
        }

        public async Task<PackingProgressDto> GetPackingProgressAsync(int userId, int tripId)
        {
            await EnsureUserOwnsTripAsync(userId, tripId);

            var items = (await _gearItemRepository.GetItemsByTripIdAsync(tripId)).ToList();

            var individualItems = items.Where(i => !i.IsGroupGear).ToList();
            var groupItems = items.Where(i => i.IsGroupGear).ToList();

            int total = items.Count;
            int packed = items.Count(i => i.IsPacked);

            int indTotal = individualItems.Count;
            int indPacked = individualItems.Count(i => i.IsPacked);

            int grpTotal = groupItems.Count;
            int grpPacked = groupItems.Count(i => i.IsPacked);

            return new PackingProgressDto
            {
                TotalItems = total,
                PackedItems = packed,
                ProgressPercentage = total == 0 ? 0 : Math.Round((double)packed / total * 100, 2),

                IndividualTotalItems = indTotal,
                IndividualPackedItems = indPacked,
                IndividualProgressPercentage = indTotal == 0 ? 0 : Math.Round((double)indPacked / indTotal * 100, 2),

                IndividualPackWeightGrams = individualItems
                    .Where(i => !i.IsWearable)
                    .Sum(i => i.WeightInGrams * i.Quantity),

                IndividualWornWeightGrams = individualItems
                    .Where(i => i.IsWearable)
                    .Sum(i => i.WeightInGrams * i.Quantity),

                GroupTotalItems = grpTotal,
                GroupPackedItems = grpPacked,
                GroupProgressPercentage = grpTotal == 0 ? 0 : Math.Round((double)grpPacked / grpTotal * 100, 2),
                GroupTotalWeightGrams = groupItems.Sum(i => i.WeightInGrams * i.Quantity)
            };
        }
    }
}
