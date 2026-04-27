using HikingGear.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.DAL.Repositories
{
    public interface IGearItemRepository
    {
        Task<IEnumerable<GearItem>> GetItemsByTripIdAsync(int tripId);
        Task AddItemsAsync(IEnumerable<GearItem> items);
        Task UpdateItemAsync(GearItem item);
        Task DeleteItemAsync(GearItem item);
    }
}
