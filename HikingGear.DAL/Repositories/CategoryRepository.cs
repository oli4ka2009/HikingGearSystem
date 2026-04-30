using HikingGear.DAL.Data;
using HikingGear.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.DAL.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GearCategory>> GetAllAsync()
        {
            return await _context.GearCategories.ToListAsync();
        }

        public async Task<GearCategory?> GetByIdAsync(int id)
        {
            return await _context.GearCategories.FindAsync(id);
        }

        public async Task<GearCategory?> GetCategoryWithItemsAsync(int id)
        {
            return await _context.GearCategories
                .Include(c => c.GearItems)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(GearCategory category)
        {
            await _context.GearCategories.AddAsync(category);
        }

        public async Task UpdateAsync(GearCategory category)
        {
            _context.GearCategories.Update(category);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(GearCategory category)
        {
            _context.GearCategories.Remove(category);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<GearCategory>> GetByTripIdAsync(int tripId)
        {
            return await _context.GearCategories
                .Include(c => c.GearItems)
                .Where(c => c.TripId == tripId)
                .ToListAsync();
        }

        public async Task<GearCategory?> GetByTripAndNameAsync(int tripId, string name)
        {
            return await _context.GearCategories
                .FirstOrDefaultAsync(c => c.TripId == tripId && c.Name == name);
        }
    }
}
