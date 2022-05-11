using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyFinances.Models
{
    public class IncomeRecords
    {
        [Key]
        public int incomeRecordID { get; set; }
        [Display(Name = "Ammount")]
        public decimal ammount { get; set; }
        [Display(Name = "Details")]
        public string details { get; set; }
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime date { get; set; }
        [ForeignKey("category")]
        public int categoryID { get; set; }
        [ForeignKey("financialAccount")]
        public int financialAccountID { get; set; }

        public virtual Category category { get; set; }
        public virtual FinancialAccount financialAccount { get; set; }
    }
}