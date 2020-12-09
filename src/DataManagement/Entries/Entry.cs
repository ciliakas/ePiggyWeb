using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
	public class Entry : IEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [StringLength(25, ErrorMessage = "Too long title!")]
        public string Title {get; set; }
        [Range(0, 99999999.99, ErrorMessage = "Amount out of range!")]
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool Recurring { get; set; }
        public int Importance { get; set; }


        public static Entry CreateLocalEntry(string title, decimal amount, DateTime date, bool recurring, int importance)
        {
            return new Entry(title, amount, date, recurring, importance);
        }

        public static Entry CreateLocalEntry(IGoal goal, DateTime date, bool recurring, int importance)
        {
            return new Entry(goal.Title, goal.Amount, date, recurring, importance);
        }

        private Entry(string title, decimal amount, DateTime date, bool recurring, int importance)
        {
            Title = title;
            Amount = amount;
            Date = date;
            Recurring = recurring;
            Importance = importance;
        }

        public Entry(int id, int userId, string title, decimal amount, DateTime date, bool recurring, int importance) 
            : this(title, amount, date, recurring, importance)
        {
            Id = id;
            UserId = userId;
        }

        public Entry(int id, int userId, IEntry entry) 
            : this(id, userId, entry.Title, entry.Amount, entry.Date, entry.Recurring, entry.Importance) { }

        public Entry(IEntryModel dbEntry) 
            : this(dbEntry.Id, dbEntry.UserId, dbEntry.Title, dbEntry.Amount, dbEntry.Date, dbEntry.IsMonthly, dbEntry.Importance) { }

        public Entry(int id, IGoal goal, DateTime date, bool recurring, int importance) 
            : this(id, goal.UserId, goal.Title, goal.Amount, date, recurring, importance) { }

        public Entry(int id, IGoalModel dbGoal, DateTime date, bool recurring, int importance)
            : this(id, dbGoal.UserId, dbGoal.Title, dbGoal.Price, date, recurring, importance) { }

        public Entry()
		{
			Id = 0;
            UserId = 0;
			Amount = 0;
			Title = "unnamed";
			Date = DateTime.Now;
			Recurring = false;
			Importance = 0;
        }

        //For simpler editing in other methods
        public void Edit(IEntry newEntry)
        {
            Title = newEntry.Title;
            Amount = newEntry.Amount;
            Date = newEntry.Date;
            Recurring = newEntry.Recurring;
            Importance = newEntry.Importance;
        }

        public void Edit(IGoal goal)
        {
            Title = goal.Title;
            Amount = goal.Amount;
        }

        public int CompareTo(IGoal other)
        {
            return other is null ? 1 : Amount.CompareTo(other.Amount);
        }

        public bool Equals(IGoal other)
        {
            if (other is null)
            {
                return false;
            }

            return Amount == other.Amount;
        }

        public int CompareTo(decimal other)
        {
            return Amount.CompareTo(other);
        }

        public bool Equals(decimal other)
        {
            return Amount == other;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var property in GetType().GetProperties())
            {
                var value = property.GetValue(this, null);
                var name = property.Name;
                sb.Append(name);
                sb.Append(": ");
                switch (value)
                {
                    case DateTime time:
                        sb.Append(time.ToShortDateString());
                        break;
                    case decimal value1:
                        sb.Append(NumberFormatter.FormatCurrency(value1));
                        break;
                    case int num:
                        if (name.Equals(nameof(Importance)))
                        {
                            sb.Append((Importance)num);
                        }
                        else // Could remove this later, to not show id and user id
                        {
                            sb.Append(value);
                        }
                        break;
                    default:
                        sb.Append(value);
                        break;
                }
                sb.Append(" ");
            }

            return sb.ToString();
        }
    }
}
