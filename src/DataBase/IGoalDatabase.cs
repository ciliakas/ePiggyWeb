using System.Collections.Generic;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;

namespace ePiggyWeb.DataBase
{
    public interface IGoalDatabase
    {
        public Task CreateAsync(IGoal goal, int userid);
        public Task CreateListAsync(IGoalList goalList, int userid);
        public Task UpdateAsync(IGoal oldGoal, IGoal newGoal);
        public Task UpdateAsync(int id, int userId, IGoal newGoal);
        public Task DeleteAsync(IGoal goal);
        public Task DeleteAsync(int id, int userId);
        public Task DeleteListAsync(IEnumerable<IGoal> goalList, int userId);
        public Task MoveGoalToExpensesAsync(IGoal goal, IEntry expense);
        public Task MoveGoalToExpensesAsync(int goalId, int userId, IEntry expense);
        public Task<IGoal> ReadAsync(int id, int userId);
        public Task<IGoalList> ReadListAsync(int userId);
    }
}
