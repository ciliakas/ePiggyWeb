﻿using System;
using System.ComponentModel.DataAnnotations;
using ePiggyWeb.DataManagement.Entries;

namespace ePiggyWeb.DataBase.Models
{
    public class ExpenseModel : IEntryModel
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

        public ExpenseModel() {}

        public ExpenseModel(Entry entry, int userId)
        {
            UserId = userId;
            Amount = entry.Amount;
            Title = entry.Title;
            Date = entry.Date;
            IsMonthly = entry.Recurring;
            Importance = entry.Importance;
        }
        public void Edit(Entry newEntry)
        {
            ((IEntryModel)this).Edit(newEntry);
        }
    }

}