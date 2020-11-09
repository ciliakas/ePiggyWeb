﻿using System;
using System.ComponentModel.DataAnnotations;
using ePiggyWeb.DataManagement.Entries;

namespace ePiggyWeb.DataBase.Models
{
    public class IncomeModel : IEntryModel
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

        public IncomeModel() {}

        public IncomeModel(IEntry entry, int userId)
        {
            UserId = userId;
            Amount = entry.Amount;
            Title = entry.Title;
            Date = entry.Date;
            IsMonthly = entry.Recurring;
            Importance = entry.Importance;
        }

        public void Edit(IEntry newEntry)
        {
            Amount = newEntry.Amount;
            Title = newEntry.Title;
            Date = newEntry.Date;
            IsMonthly = newEntry.Recurring;
            Importance = newEntry.Importance;
        }
    }

}