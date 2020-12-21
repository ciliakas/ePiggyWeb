using System.Collections.Generic;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;

namespace ePiggyWeb.DataBase
{
    public interface IGoalDatabase
    {
        public Task<int> CreateAsync(IGoal goal, int userid);
        public Task<bool> CreateListAsync(IGoalList goalList, int userid);
        public Task<bool> UpdateAsync(IGoal oldGoal, IGoal newGoal);
        public Task<bool> UpdateAsync(int id, int userId, IGoal newGoal);
        public Task<bool> DeleteAsync(IGoal goal);
        public Task<bool> DeleteAsync(int id, int userId);
        public Task<bool> DeleteListAsync(IEnumerable<IGoal> goalList, int userId);
        public Task<int> MoveGoalToExpensesAsync(IGoal goal, IEntry expense);
        public Task<int> MoveGoalToExpensesAsync(int goalId, int userId, IEntry expense);
        public Task<IGoal> ReadAsync(int id, int userId);
        public Task<IGoalList> ReadListAsync(int userId);
    }
}
