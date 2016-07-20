using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MAPI.Models
{
    public class Account
    {
        public string Location { get; set; }

        public int ID { get; set; }

        public ICollection<AccountType> AccountTypes;
    }

    public class AccountType
    {
        public virtual Account Account { get; set; }

        public string ID { get; set; }

        public int AccountID { get; set; }

        public int AuthType { get; set; }
    }

    public class Login
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}