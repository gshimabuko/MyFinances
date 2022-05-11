using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyFinances.Models
{
    public enum CategoryType { Income = 0, Expense = 1 }
    public class Category
    {
        [Key]
        public int categoryID { get; set; }
        [Display(Name = "Category")]
        public string name { get; set; }
        [Display(Name = "Total")]
        public decimal total { get; set; }
        [Display(Name = "Category Type")]
        public CategoryType categoryType { get; set; }

        public virtual ICollection<IncomeRecords> incomeRecords { get; set; }
        public virtual ICollection<ExpenseRecords> expenseRecords { get; set; }

        public Category()
        {
            this.name = "Default Category";
            this.total = 0;
        }
        public Category(string name)
        {
            this.name = name;
            this.total = 0;
        }
    }
}