using System.Collections.Generic;
using System.Text;

namespace ePiggyWeb.DataManagement.Goals
{
    public class GoalList : List<Goal>
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
    }
}
