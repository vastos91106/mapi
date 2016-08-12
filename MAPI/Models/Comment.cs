namespace MAPI.Models
{
    public class Comment
    {
        public int ID { get; set; }

        public int? To { get; set; }

        public string Text { get; set; }

        public int MarkID { get; set; }

        public virtual Mark Mark { get; set; }
    }
}