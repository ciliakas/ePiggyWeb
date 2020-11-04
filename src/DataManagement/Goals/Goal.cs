using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

        public Goal(string title, decimal amount)
        {
            Title = title;
            Amount = amount;
        }

        public Goal(int id, int userId, string title, decimal amount) : this(title, amount)
        {
            Id = id;
            UserId = userId;
        }

        public Goal(IGoalModel dbGoalModel) : this(dbGoalModel.Id, dbGoalModel.UserId, dbGoalModel.Title, dbGoalModel.Price) { }

        public Goal(int id, int userId, IGoal goal) : this(id, userId, goal.Title, goal.Amount) { }

        public Goal()
        {
            Id = 0;
            UserId = 0;
            Title = "unnamed";
            Amount = 0;
        }

        public void Edit(IGoal goal)
        {
            Title = goal.Title;
            Amount = goal.Amount;
        }

        // Since I don't know how this should work with the threads and so on, I will keep this commented for the time being

        //public Goal(string title)
        //{
        //    SetGoalFromWeb(title);
        //}

        //private static readonly string ResourceDirectoryParsedGoal = Directory.GetParent(Environment.CurrentDirectory)
        //                                                                 .Parent?.Parent?.FullName +
        //                                                             @"\resources\textData\parsedGoal.txt";
        //public void SetGoalFromWeb(string itemName)
        //{
        //    try
        //    {
        //Below line should be amended, as the teacher said this is not the correct way to use threads
        //        Task.Run(() => InternetParser.ReadPriceFromCamel(itemName)).Wait();

        //        var file = new StreamReader(ResourceDirectoryParsedGoal);
        //        file.ReadLine();
        //        Title = file.ReadLine();
        //        var priceString = file.ReadLine();
        //        Amount = Convert.ToDecimal(priceString, System.Globalization.CultureInfo.CurrentCulture);
        //        file.Close();
        //    }
        //    catch (Exception e)
        //    {
        //        ExceptionHandler.Log(e.ToString());
        //    }
        //}


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
