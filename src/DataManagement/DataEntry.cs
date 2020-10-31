﻿using System;
using System.Text;
using ePiggyWeb.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ePiggyWeb.DataManagement
{
	public class DataEntry : IComparable, IEquatable<DataEntry>
	{
        /*Properties*/
        public string Title {get; set; }
        public decimal Amount { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public bool IsMonthly { get; set; }
        public int Importance { get; set; }

		/*Constructors*/
		public DataEntry(decimal amount, string title, DateTime date, bool isMonthly, int importance)
        {
            Amount = amount;
            Title = title;
            Date = date;
            IsMonthly = isMonthly;
            Importance = importance;
        }

        public DataEntry(int id, int userId, decimal amount, string title, DateTime date, bool isMonthly, int importance)
            :this(amount, title, date, isMonthly, importance)
		{
			Id = id;
            UserId = userId;
        }

		public DataEntry()
		{
			Id = 0;
            UserId = 0;
			Amount = 0;
			Title = "unnamed";
			Date = DateTime.Now;
			IsMonthly = false;
			Importance = 0;
		}

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var property in GetType().GetProperties())
            {
                var value = property.GetValue(this, null);
                var name = property.Name;
                sb.Append(property.Name);
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
                            sb.Append((Importance) num);
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

        public int CompareTo(object obj)
        {
            return obj switch
            {
                null => 1,
                DataEntry otherEntry => Amount.CompareTo(otherEntry.Amount),
                _ => throw new ArgumentException("Object is not a DataEntry")
            };
        }

        public bool Equals(DataEntry other)
        {
            if (other is null)
            {
                return false;
            }

            return Amount == other.Amount && Importance == other.Importance && Title.Equals(other.Title);
        }
    }
}
