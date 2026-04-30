using HikingGear.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.DAL.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<GearCategory>> GetAllAsync();
        Task<GearCategory?> GetByIdAsync(int id);
        Task<GearCategory?> GetCategoryWithItemsAsync(int id);
        Task AddAsync(GearCategory category);
        Task UpdateAsync(GearCategory category);
        Task DeleteAsync(GearCategory category);
        Task SaveChangesAsync();
    }
}
