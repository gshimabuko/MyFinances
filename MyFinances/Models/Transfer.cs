using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyFinances.Models
{
    public class Transfer
    {
        [Key]
        public int transferID { get; set; }
        [Display(Name = "Source Account")]
        [ForeignKey("sourceAccount")]
        public int sourceAccountID { get; set; }
        [Display(Name = "Destination Account")]
        [ForeignKey("destinationAccount")]
        public int? destinationAccountID { get; set; }
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime date { get; set; }
        [Display(Name = "Ammount")]
        public decimal ammount { get; set; }
        [Display(Name = "Exchange Rate")]
        public decimal exchangeRate { get; set; }

        public virtual FinancialAccount sourceAccount { get; set; }
        public virtual FinancialAccount destinationAccount { get; set; }
    }
}