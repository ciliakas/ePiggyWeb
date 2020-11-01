using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ePiggy.Utilities;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Goals
{
    public class Goal : IEquatable<Goal>, IComparable
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; }

        public decimal Price { get; set; }


        public Goal(int id, int userId, string title, decimal price)
            :this(title, price)
        {
            Id = id;
            UserId = userId;
        }

        public Goal(string title, decimal price)
        {
            Title = title;
            Price = price;
        }

        public Goal()
        {
            Id = 0;
            UserId = 0;
            Title = "unnamed";
            Price = 0;
        }

        public void Edit(Goal goal)
        {
            Title = goal.Title;
            Price = goal.Price;
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
                Price = Convert.ToDecimal(priceString, System.Globalization.CultureInfo.CurrentCulture);
                file.Close();
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e.ToString());
            }
        }


        public bool Equals(Goal other)
        {
            if (other is null)
            {
                return false;
            }

            return Price == other.Price;
        }

        public int CompareTo(object? obj)
        {
            return obj switch
            {
                null => 1,
                Goal otherGoal => Price.CompareTo(otherGoal.Price),
                _ => throw new ArgumentException("Object is not a Goal")
            };
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
