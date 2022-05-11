using Microsoft.AspNet.Identity.EntityFramework;
using MyFinances.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MyFinances.Data
{
    public class MyFinancesContext : IdentityDbContext<ApplicationUser>
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public MyFinancesContext() : base("name=MyFinancesContext")
        {
            this.Configuration.LazyLoadingEnabled = true;
        }

        public System.Data.Entity.DbSet<MyFinances.Models.Category> Categories { get; set; }

        public System.Data.Entity.DbSet<MyFinances.Models.FinancialAccount> FinancialAccounts { get; set; }

        public System.Data.Entity.DbSet<MyFinances.Models.IncomeRecords> IncomeRecords { get; set; }

        public System.Data.Entity.DbSet<MyFinances.Models.ExpenseRecords> ExpenseRecords { get; set; }

        public System.Data.Entity.DbSet<MyFinances.Models.Transfer> Transfers { get; set; }
    }
}
