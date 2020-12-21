using System.Collections.Generic;
using System.Linq;
using System.Text;
using ePiggyWeb.CurrencyAPI;
using Microsoft.Extensions.Configuration;

namespace ePiggyWeb.DataManagement.Goals
{
    public class GoalList : List<IGoal>, IGoalList
    {
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var goal in this)
            {
                sb.Append(goal);
            }

            return sb.ToString();
        }

        public static IGoalList RandomList(IConfiguration configuration)
        {
            IGoalList list = new GoalList();
            var section = configuration.GetSection("Goal");

            foreach (var configurationSection in section.GetChildren())
            {
                var sec = configurationSection.GetChildren();
                var configurationSections = sec as IConfigurationSection[] ?? sec.ToArray();
                IGoal goal = Goal.CreateLocalGoal(title: configurationSections.Last().Value,
                   amount: decimal.Parse(configurationSections.First().Value), currency: Currency.DefaultCurrencyCode);
                list.Add(goal);
            }

            return list;
        }
    }
}
