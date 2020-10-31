using System;
using System.ComponentModel.DataAnnotations;
using ePiggyWeb.DataManagement;

namespace ePiggyWeb.DataBase.Models
{
    public class Incomes
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

        public Incomes(Entry entry, int userId)
        {
            UserId = userId;
            Amount = entry.Amount;
            Title = entry.Title;
            Date = entry.Date;
            IsMonthly = entry.IsMonthly;
            Importance = entry.Importance;
        }
        public void Edit(Entry newEntry)
        {
            Amount = newEntry.Amount;
            Title = newEntry.Title;
            Date = newEntry.Date;
            IsMonthly = newEntry.IsMonthly;
            Importance = newEntry.Importance;
        }
    }

}