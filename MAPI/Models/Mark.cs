using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace MAPI.Models
{
    public class Mark
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [Range(double.MinValue, double.MaxValue)]
        public double Lat { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [Range(double.MinValue, double.MaxValue)]
        public double Lon { get; set; }

        public float Radius { get; set; }

        public int Rating { get; set; }

        public string Description { get; set; }

        public int AccountID { get; set; }

        public virtual Account Account { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}