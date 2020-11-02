using System;
using System.Text;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
	public class Entry : IFinanceable, IComparable<IFinanceable>, IComparable<decimal>, IEquatable<IFinanceable>, IEquatable<decimal>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title {get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool Recurring { get; set; }
        public int Importance { get; set; }


        public Entry(string title, decimal amount,  DateTime date, bool recurring, int importance)
        {
            Title = title;
            Amount = amount;
            Date = date;
            Recurring = recurring;
            Importance = importance;
        }

        public Entry(int id, int userId, string title, decimal amount,  DateTime date, bool recurring, int importance)
            : this(title, amount, date, recurring, importance)
        {
            Id = id;
            UserId = userId;
        }


        public Entry(int id, int userId, Entry entry)
            :this(entry.Title, entry.Amount, entry.Date, entry.Recurring, entry.Importance)
        {
            Id = id;
            UserId = userId;
        }

        public Entry(IncomeModel dbEntry) :this(dbEntry.Id, dbEntry.UserId, dbEntry.Title, dbEntry.Amount, dbEntry.Date, dbEntry.IsMonthly, dbEntry.Importance) { }

        public Entry(ExpenseModel dbEntry) : this(dbEntry.Id, dbEntry.UserId, dbEntry.Title, dbEntry.Amount, dbEntry.Date, dbEntry.IsMonthly, dbEntry.Importance) { }

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
        public void Edit(Entry newEntry)
        {
            Title = newEntry.Title;
            Amount = newEntry.Amount;
            Date = newEntry.Date;
            Recurring = newEntry.Recurring;
            Importance = newEntry.Importance;
        }

        public int CompareTo(IFinanceable other)
        {
            return other is null ? 1 : Amount.CompareTo(other.Amount);
        }

        public int CompareTo(decimal other)
        {
            return Amount.CompareTo(other);
        }

        public bool Equals(IFinanceable other)
        {
            if (other is null)
            {
                return false;
            }

            return Amount == other.Amount;
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
