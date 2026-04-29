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
        [JsonPropertyName("categories")]
        public List<TripCategoryDto> Categories { get; set; } = new();
    }

    public class TripCategoryDto
    {
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<TripGearItemDto> Items { get; set; } = new();
    }

    public class TripGearItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("weightInGrams")]
        public double WeightInGrams { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("isGroupGear")]
        public bool IsGroupGear { get; set; }

        [JsonPropertyName("isWearable")]
        public bool IsWearable { get; set; }

        [JsonPropertyName("isPacked")]
        public bool IsPacked { get; set; }
    }
}
