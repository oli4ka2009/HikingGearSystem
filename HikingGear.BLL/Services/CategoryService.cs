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
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITripRepository _tripRepository;

        public CategoryService(ICategoryRepository categoryRepository, ITripRepository tripRepository)
        {
            _categoryRepository = categoryRepository;
            _tripRepository = tripRepository;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                TripId = c.TripId
            });
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return new CategoryDto { Id = category.Id, Name = category.Name, TripId = category.TripId };
        }

        public async Task<CategoryDto> CreateAsync(int userId, CreateCategoryDto dto)
        {
            // Перевірка прав власності на похід
            var isOwner = await _tripRepository.IsUserOwnerOfTripAsync(dto.TripId, userId);
            if (!isOwner)
                throw new UnauthorizedAccessException("Ви не маєте доступу до цього походу.");

            // Перевірка на дублікат назви в межах цього походу
            var existing = await _categoryRepository.GetByTripAndNameAsync(dto.TripId, dto.Name);
            if (existing != null)
                throw new InvalidOperationException($"Категорія з назвою '{dto.Name}' вже існує в цьому поході.");

            var category = new GearCategory
            {
                Name = dto.Name,
                TripId = dto.TripId
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return new CategoryDto { Id = category.Id, Name = category.Name, TripId = category.TripId };
        }

        public async Task<bool> UpdateAsync(int userId, int id, UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) throw new KeyNotFoundException("Категорію не знайдено.");

            // Перевірка прав власності
            var isOwner = await _tripRepository.IsUserOwnerOfTripAsync(category.TripId, userId);
            if (!isOwner)
                throw new UnauthorizedAccessException("Ви не маєте доступу до цієї категорії.");

            // Перевірка на дублікат (якщо назва змінилась)
            if (category.Name != dto.Name)
            {
                var existing = await _categoryRepository.GetByTripAndNameAsync(category.TripId, dto.Name);
                if (existing != null)
                    throw new InvalidOperationException($"Категорія з назвою '{dto.Name}' вже існує.");
            }

            category.Name = dto.Name;

            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int userId, int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            // Перевірка прав власності
            var isOwner = await _tripRepository.IsUserOwnerOfTripAsync(category.TripId, userId);
            if (!isOwner)
                throw new UnauthorizedAccessException("Ви не маєте доступу до цієї категорії.");

            await _categoryRepository.DeleteAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return true;
        }
    }
}
