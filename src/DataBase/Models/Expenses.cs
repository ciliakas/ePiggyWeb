using System;
using System.ComponentModel.DataAnnotations;

namespace ePiggyWeb.DataBase.Models
{
    public class Expenses
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        [StringLength(255)]
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public bool IsMonthly { get; set; }
        public int Importance { get; set; }

    }

}