using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
