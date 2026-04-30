using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL.DTOs
{
    public class GearItemCreateDto
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Назва речі є обов'язковою")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100000)]
        public double WeightInGrams { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }

        public bool IsGroupGear { get; set; }
        public bool IsWearable { get; set; }
    }

    public class GearItemUpdateDto
    {
        [Required(ErrorMessage = "Назва речі є обов'язковою")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100000)]
        public double WeightInGrams { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }

        public bool IsGroupGear { get; set; }
        public bool IsWearable { get; set; }
    }

    public class GearItemPackStatusDto
    {
        public bool IsPacked { get; set; }
    }

    public class PackingProgressDto
    {
        public int TotalItems { get; set; }
        public int PackedItems { get; set; }
        public double ProgressPercentage { get; set; }

        public int IndividualTotalItems { get; set; }
        public int IndividualPackedItems { get; set; }
        public double IndividualProgressPercentage { get; set; }

        public double IndividualPackWeightGrams { get; set; } 
        public double IndividualWornWeightGrams { get; set; }

        public int GroupTotalItems { get; set; }
        public int GroupPackedItems { get; set; }
        public double GroupProgressPercentage { get; set; }
        public double GroupTotalWeightGrams { get; set; }
    }
}
