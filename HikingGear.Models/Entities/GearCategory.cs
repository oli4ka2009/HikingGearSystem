using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.Models.Entities
{
    public class GearCategory
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<GearItem> GearItems { get; set; } = new List<GearItem>();
    }
}
