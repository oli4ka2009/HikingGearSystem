using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.Models.Entities
{
    public enum SleepFormat
    {
        None,       
        Tent,       
        Shelter     
    }

    public class Trip
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LocationName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int GroupSize { get; set; }

        public SleepFormat AccommodationFormat { get; set; }

        public ICollection<GearItem> GearItems { get; set; } = new List<GearItem>();
        public ICollection<GearCategory> Categories { get; set; } = new List<GearCategory>();
    }
}
