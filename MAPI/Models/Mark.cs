using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace MAPI.Models
{
    public class Mark
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Avatar { get; set; }

        [Required]
        public double Lat { get; set; }

        [Required]
        public double Lon { get; set; }

        public float Radius { get; set; }

        public int Rating { get; set; }

        public string Description { get; set; }

        public int AccountID { get; set; }

        public virtual Account Account { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}