using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.Models.Entities
{
    public class GearItem
    {
        public int Id { get; set; }

        public int TripId { get; set; }
        public Trip Trip { get; set; } = null!;

        public int CategoryId { get; set; }
        public GearCategory Category { get; set; } = null!;

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public double WeightInGrams { get; set; }

        public int Quantity { get; set; }

        public bool IsGroupGear { get; set; }
        public bool IsWearable { get; set; }
        public bool IsPacked { get; set; }
    }
}
