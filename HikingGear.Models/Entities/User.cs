using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(512)]
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
