using System.Data.Entity;

namespace MAPI.Models
{
    public class Context : DbContext
    {
        public Context()
            : base("DefaultConnection")
        {

        }
        public virtual DbSet<Account> Accounts { get; set; }

        public virtual DbSet<AccountType> AccountTypes { get; set; }

        public virtual DbSet<Mark> Marks { get; set; }

        public virtual  DbSet<Comment> MarkComments { get; set; } 
    }
}