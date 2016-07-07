using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MAPI.Models
{
    public class  Context :  DbContext
    {
        public virtual  DbSet<Account> Accounts { get; set; }
    }
}