using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;
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
                //sb.Append("\n");
            }

            return sb.ToString();
        }
        public static IGoalList RandomList(IConfiguration configuration)
        {
            IGoalList list = new GoalList();
            var section = configuration.GetSection("Goal");

            foreach (var thing in section.GetChildren())
            {
                var sec = thing.GetChildren();
                var configurationSections = sec as IConfigurationSection[] ?? sec.ToArray();
                IGoal goal = Goal.CreateLocalGoal(configurationSections.Last().Value, decimal.Parse(configurationSections.First().Value));
                list.Add(goal);
            }

            return list;
        }
    }
}
