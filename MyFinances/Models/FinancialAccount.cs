using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyFinances.Models
{
    
    public class FinancialAccount
    {
        [Key]
        public int financialAccountID { get; set; }
        [Display(Name = "Name")]
        public string name { get; set; }
        [Display(Name = "Account Balance")]
        public decimal accountBalance { get; set; }
        [Display(Name = "Currency")]
        public string currency { get; set; }
        public string OwnerID { get; set; }
        public virtual ICollection<IncomeRecords> incomeRecords { get; set; }
        [Display(Name = "Expenses")]
        public virtual ICollection<ExpenseRecords> expenseRecords { get; set; }
        [Display(Name = "Inbound Transfers")]
        public virtual ICollection<Transfer> transferSource { get; set; }
        [Display(Name = "Outbound Transfers")]
        public virtual ICollection<Transfer> transferDestination { get; set; }

        public void Deposit(decimal ammount)
        {
            this.accountBalance += ammount;
        }
        public void Withdraw (decimal ammount)
        {
            this.accountBalance -= ammount;
        }
    }
}