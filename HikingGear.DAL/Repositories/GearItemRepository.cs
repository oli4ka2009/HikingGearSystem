using Dapper;
using HikingGear.DAL.Data;
using HikingGear.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.DAL.Repositories
{
    public class GearItemRepository : IGearItemRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public GearItemRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<IEnumerable<GearItem>> GetItemsByTripIdAsync(int tripId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
                SELECT g.*, c.Id, c.Name 
                FROM GearItems g
                INNER JOIN GearCategories c ON g.CategoryId = c.Id
                WHERE g.TripId = @TripId";

            return await connection.QueryAsync<GearItem, GearCategory, GearItem>(
                query,
                (gear, category) =>
                {
                    gear.Category = category;
                    return gear;
                },
                new { TripId = tripId },
                splitOn: "Id"
            );
        }

        public async Task<int> GetOrCreateCategoryIdAsync(string categoryName)
        {
            var category = await _context.GearCategories
                .FirstOrDefaultAsync(c => c.Name == categoryName);

            if (category == null)
            {
                category = new GearCategory { Name = categoryName };
                await _context.GearCategories.AddAsync(category);
                await _context.SaveChangesAsync();
            }

            return category.Id;
        }

        public async Task AddItemsAsync(IEnumerable<GearItem> items)
        {
            await _context.GearItems.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(GearItem item)
        {
            _context.GearItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(GearItem item)
        {
            _context.GearItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task<GearItem?> GetItemByIdAsync(int itemId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM GearItems WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<GearItem>(query, new { Id = itemId });
        }

        public async Task DeleteItemsByTripIdAsync(int tripId)
        {
            var itemsToDelete = await _context.GearItems
                .Where(i => i.TripId == tripId)
                .ToListAsync();

            if (itemsToDelete.Any())
            {
                _context.GearItems.RemoveRange(itemsToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}
