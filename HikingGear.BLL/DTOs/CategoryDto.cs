using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TripId { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Назва категорії обов'язкова")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public int TripId { get; set; }
    }

    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Назва категорії обов'язкова")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
