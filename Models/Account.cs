using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MAPI.Models
{
    public class Account
    {
        public string Location { get; set; }

        public string ID { get; set; }
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