using System.Collections.Generic;

namespace MAPI.Models
{
    public class Mark
    {
        public int MarkID { get; set; }

        public string Name { get; set; }

        public string Avatar { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Radius { get; set; }

        public int Rating { get; set; }

        public int Visited { get; set; }

        public int AccountID { get; set; }

        public virtual Account Owner { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}