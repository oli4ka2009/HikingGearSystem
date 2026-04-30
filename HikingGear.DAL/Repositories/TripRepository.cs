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
    public class TripRepository : ITripRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public TripRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<Trip?> GetTripByIdAsync(int id)
        {
            return await _context.Trips
                .Include(t => t.Categories)
                    .ThenInclude(c => c.GearItems)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Trip>> GetUserTripsAsync(int userId)
        {
            return await _context.Trips
                .Include(t => t.Categories)
                    .ThenInclude(c => c.GearItems)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }

        public async Task AddTripAsync(Trip trip)
        {
            await _context.Trips.AddAsync(trip);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTripAsync(Trip trip)
        {
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTripAsync(Trip trip)
        {
            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUserOwnerOfTripAsync(int tripId, int userId)
        {
            return await _context.Trips
                .AnyAsync(t => t.Id == tripId && t.UserId == userId);
        }
    }
}
