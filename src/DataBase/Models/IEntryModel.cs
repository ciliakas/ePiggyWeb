using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement.Entries;

namespace ePiggyWeb.DataBase.Models
{
    public interface IEntryModel
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

        public void Edit(Entry newEntry)
        {
            Amount = newEntry.Amount;
            Title = newEntry.Title;
            Date = newEntry.Date;
            IsMonthly = newEntry.Recurring;
            Importance = newEntry.Importance;
        }
    }
}
