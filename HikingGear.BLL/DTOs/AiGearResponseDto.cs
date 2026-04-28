using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HikingGear.BLL.DTOs
{
    public class AiGearResponseDto
    {
        [JsonPropertyName("categories")]
        public List<AiCategoryDto> Categories { get; set; } = new();
    }

    public class AiCategoryDto
    {
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<AiGearItemDto> Items { get; set; } = new();
    }

    public class AiGearItemDto
    {
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
    }
}
