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
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITripRepository _tripRepository;

        public GearItemService(
            IGearItemRepository gearItemRepository,
            ICategoryRepository categoryRepository,
            ITripRepository tripRepository)
        {
            _gearItemRepository = gearItemRepository;
            _categoryRepository = categoryRepository;
            _tripRepository = tripRepository;
        }

        private async Task EnsureUserOwnsCategoryAsync(int userId, int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new KeyNotFoundException("Категорію не знайдено.");

            var isOwner = await _tripRepository.IsUserOwnerOfTripAsync(category.TripId, userId);
            if (!isOwner)
                throw new UnauthorizedAccessException("Ви не маєте доступу до цієї категорії.");
        }

        public async Task<GearItem> AddCustomGearItemAsync(int userId, GearItemCreateDto dto)
        {
            await EnsureUserOwnsCategoryAsync(userId, dto.CategoryId);

            var newItem = new GearItem
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                WeightInGrams = dto.WeightInGrams,
                Quantity = dto.Quantity,
                IsGroupGear = dto.IsGroupGear,
                IsWearable = dto.IsWearable,
                IsPacked = false
            };

            await _gearItemRepository.AddItemsAsync(new List<GearItem> { newItem });
            return newItem;
        }

        public async Task UpdateGearItemAsync(int userId, int itemId, GearItemUpdateDto dto)
        {
            var item = await _gearItemRepository.GetItemByIdAsync(itemId);
            if (item == null) throw new KeyNotFoundException("Річ не знайдено.");

            await EnsureUserOwnsCategoryAsync(userId, item.CategoryId);

            item.Name = dto.Name;
            item.WeightInGrams = dto.WeightInGrams;
            item.Quantity = dto.Quantity;
            item.IsGroupGear = dto.IsGroupGear;
            item.IsWearable = dto.IsWearable;

            await _gearItemRepository.UpdateItemAsync(item);
        }

        public async Task DeleteGearItemAsync(int userId, int itemId)
        {
            var item = await _gearItemRepository.GetItemByIdAsync(itemId);
            if (item == null) throw new KeyNotFoundException("Річ не знайдено.");

            await EnsureUserOwnsCategoryAsync(userId, item.CategoryId);
            await _gearItemRepository.DeleteItemAsync(item);
        }

        public async Task UpdatePackedStatusAsync(int userId, int itemId, bool isPacked)
        {
            var item = await _gearItemRepository.GetItemByIdAsync(itemId);
            if (item == null) throw new KeyNotFoundException("Річ не знайдено.");

            await EnsureUserOwnsCategoryAsync(userId, item.CategoryId);
            item.IsPacked = isPacked;
            await _gearItemRepository.UpdateItemAsync(item);
        }

        public async Task<TripGearResponseDto> GetGearForTripAsync(int tripId, int userId)
        {
            var trip = await _tripRepository.GetTripByIdAsync(tripId); // з Include Categories + ThenInclude GearItems

            if (trip == null)
                throw new KeyNotFoundException("Похід не знайдено.");

            if (trip.UserId != userId)
                throw new UnauthorizedAccessException();

            return new TripGearResponseDto
            {
                Categories = trip.Categories.Select(c => new TripCategoryDto
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

        public async Task<PackingProgressDto> GetPackingProgressAsync(int userId, int tripId)
        {
            // Перевіряємо, що похід належить юзеру
            var trip = await _tripRepository.GetTripByIdAsync(tripId);
            if (trip == null || trip.UserId != userId)
                throw new UnauthorizedAccessException("Доступ заборонено.");

            // ✅ Дістаємо всі речі через категорії походу
            var items = trip.Categories
                .SelectMany(c => c.GearItems)
                .ToList();

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
                IndividualPackWeightGrams = individualItems.Where(i => !i.IsWearable).Sum(i => i.WeightInGrams * i.Quantity),
                IndividualWornWeightGrams = individualItems.Where(i => i.IsWearable).Sum(i => i.WeightInGrams * i.Quantity),
                GroupTotalItems = grpTotal,
                GroupPackedItems = grpPacked,
                GroupProgressPercentage = grpTotal == 0 ? 0 : Math.Round((double)grpPacked / grpTotal * 100, 2),
                GroupTotalWeightGrams = groupItems.Sum(i => i.WeightInGrams * i.Quantity)
            };
        }
    }
}
