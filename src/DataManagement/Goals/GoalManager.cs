using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ePiggyWeb.DataManagement.Goals
{
    public class GoalManager
    {
        public GoalList GoalList { get; }

        private int UserId { get; } = 0;

        public GoalManager()
        {
            GoalList = new GoalList();
        }
    }
}
