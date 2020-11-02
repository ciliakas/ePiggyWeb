using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ePiggy.Utilities;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Goals
{
    public class Goal : IFinanceable, IComparable<IFinanceable>, IComparable<decimal>, IEquatable<IFinanceable>, IEquatable<decimal>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }

        public Goal(string title, decimal amount)
        {
            Title = title;
            Amount = amount;
        }

        public Goal(int id, int userId, string title, decimal amount)
            :this(title, amount)
        {
            Id = id;
            UserId = userId;
        }


        public Goal(IGoalModel dbGoalModel) :this(dbGoalModel.Id, dbGoalModel.UserId, dbGoalModel.Title, dbGoalModel.Amount) { }

        public Goal()
        {
            Id = 0;
            UserId = 0;
            Title = "unnamed";
            Amount = 0;
        }

        public void Edit(Goal goal)
        {
            Title = goal.Title;
            Amount = goal.Amount;
        }

        // This needs to be amended

        public Goal(string title)
        {
            SetGoalFromWeb(title);
        }

        private static readonly string ResourceDirectoryParsedGoal = Directory.GetParent(Environment.CurrentDirectory)
                                                                         .Parent?.Parent?.FullName +
                                                                     @"\resources\textData\parsedGoal.txt";
        public void SetGoalFromWeb(string itemName)
        {
            try
            {
                Task.Run(() => InternetParser.ReadPriceFromCamel(itemName)).Wait();

                var file = new StreamReader(ResourceDirectoryParsedGoal);
                file.ReadLine();
                Title = file.ReadLine();
                var priceString = file.ReadLine();
                Amount = Convert.ToDecimal(priceString, System.Globalization.CultureInfo.CurrentCulture);
                file.Close();
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e.ToString());
            }
        }

        //

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
                    case decimal value1:
                        sb.Append(NumberFormatter.FormatCurrency(value1));
                        break;
                    default:
                        sb.Append(value);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
