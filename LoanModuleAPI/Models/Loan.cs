using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LoanModuleAPI.Models
{
    public class Loan
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public long LoanID { get; set; }

        [Required]
        [Column(TypeName = "Decimal(10,2)")]
        public Decimal Amount { get; set; }

        [Required]
        public int Tenor { get; set; }

        public string UserID { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime CreatedDate { get; set; }

    }
}
