using System.Text;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Goals
{
    public class Goal : IGoal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public static Goal CreateLocalGoal(string title, decimal amount, string currency)
        {
            return new Goal(title, amount, currency);
        }

        private Goal(string title, decimal amount, string currency)
        {
            Title = title;
            Amount = amount;
            Currency = currency;
        }

        public Goal() { }

        public Goal(int id, int userId, string title, decimal amount, string currency) : this(title, amount, currency)
        {
            Id = id;
            UserId = userId;
        }

        public Goal(IGoalModel dbGoal) : this(dbGoal.Id, dbGoal.UserId, dbGoal.Title, dbGoal.Price, dbGoal.Currency) { }

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
                    case decimal value1:
                        sb.Append(NumberFormatter.FormatCurrency(value1));
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
