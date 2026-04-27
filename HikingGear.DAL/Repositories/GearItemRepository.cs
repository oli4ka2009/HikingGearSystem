using Dapper;
using HikingGear.DAL.Data;
using HikingGear.Models.Entities;
using Microsoft.Data.SqlClient;
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
    }
}
