using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ePiggyWeb.DataManagement.Goals
{
    public interface IGoalList : IList<IGoal>
    {
        /*
        I'm leaving this interface empty for now, even though we could just use IList<IGoal> instead for this
        But if in the future we decide that a list of goals needs to implement any more properties or methods,
        We'd have to go all over the code and change all occurrences of IList<IGoal> which would take a lot of time
         */
    }
}
