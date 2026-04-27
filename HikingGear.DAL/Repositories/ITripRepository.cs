using HikingGear.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.DAL.Repositories
{
    public interface ITripRepository
    {
        Task<Trip?> GetTripByIdAsync(int id);
        Task<IEnumerable<Trip>> GetUserTripsAsync(int userId);

        Task AddTripAsync(Trip trip);
        Task UpdateTripAsync(Trip trip);
        Task DeleteTripAsync(Trip trip);
    }
}
