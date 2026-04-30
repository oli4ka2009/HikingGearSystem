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
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<IEnumerable<GearItem>> GetItemsByCategoryIdAsync(int categoryId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM GearItems WHERE CategoryId = @CategoryId";
            return await connection.QueryAsync<GearItem>(query, new { CategoryId = categoryId });
        }

        public async Task<GearItem?> GetItemByIdAsync(int itemId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<GearItem>(
                "SELECT * FROM GearItems WHERE Id = @Id", new { Id = itemId });
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

        public async Task DeleteItemsByCategoryIdAsync(int categoryId)
        {
            var items = await _context.GearItems
                .Where(i => i.CategoryId == categoryId)
                .ToListAsync();
            _context.GearItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
