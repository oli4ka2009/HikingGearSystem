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
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM Trips WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Trip>(query, new { Id = id });
        }

        public async Task<IEnumerable<Trip>> GetUserTripsAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM Trips WHERE UserId = @UserId ORDER BY StartDate DESC";
            return await connection.QueryAsync<Trip>(query, new { UserId = userId });
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
    }
}
