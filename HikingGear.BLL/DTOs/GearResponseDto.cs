using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HikingGear.BLL.DTOs
{
    public class TripGearResponseDto
    {
        public List<TripCategoryDto> Categories { get; set; } = new();
    }

    public class TripCategoryDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<TripGearItemDto> Items { get; set; } = new();
    }

    public class TripGearItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double WeightInGrams { get; set; }
        public int Quantity { get; set; }
        public bool IsGroupGear { get; set; }
        public bool IsWearable { get; set; }
        public bool IsPacked { get; set; }
    }
}
