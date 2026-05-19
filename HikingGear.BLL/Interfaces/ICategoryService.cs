using HikingGear.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(int userId, CreateCategoryDto dto);
        Task<bool> UpdateAsync(int userId, int id, UpdateCategoryDto dto);
        Task<bool> DeleteAsync(int userId, int id);
    }
}
