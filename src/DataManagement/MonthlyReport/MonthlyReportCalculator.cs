using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.MonthlyReport
{
    public class MonthlyReportCalculator
    {
        private int UserId { get; }
        private GoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }

        private IEntryList Expenses { get; set; }
        private IEntryList Income { get; set; }
        private decimal AllSavings { get; set; }
        private IGoalList Goals { get; set; }

        private MonthlyReportResult Result { get; set; }



        public MonthlyReportCalculator(GoalDatabase goalDatabase, EntryDatabase entryDatabase, int userId)
        {
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            UserId = userId;
        }

        public async Task<MonthlyReportResult> Calculate()
        { 
            Result = await GatherDataAsync();
            CalculateBiggestExpensesCategory();
            Result.SavedUpGoals = CalculateSavedUpGoals();
            CalculateGoalReport();
            return Result;
        }

        private async Task<MonthlyReportResult> GatherDataAsync()
        {
            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            var first = month.AddMonths(-1);
            var last = month.AddDays(-1);
            var expenses = (await EntryDatabase.ReadListAsync(UserId, EntryType.Expense)).GetSum();
            var income = (await EntryDatabase.ReadListAsync(UserId, EntryType.Income)).GetSum();
            AllSavings = income - expenses;
            Expenses = (await EntryDatabase.ReadListAsync(UserId, EntryType.Expense)).GetFrom(first).GetTo(last);
            Income = (await EntryDatabase.ReadListAsync(UserId, EntryType.Income)).GetFrom(first).GetTo(last);
            Goals = await GoalDatabase.ReadListAsync(UserId);
            var balance = Income.GetSum() - Expenses.GetSum();
            return new MonthlyReportResult(Expenses.GetSum(), Income.GetSum(), balance, first, last);

        }

        private void CalculateBiggestExpensesCategory()
        {
            var enumCount = Enum.GetValues(typeof(Importance)).Length;
            var biggestCategory = (Importance) enumCount;
            var sum = 0m;
            for (var i = enumCount; i > (int) Importance.Necessary; i--)
            {
                var temp = 0M;
                var expenses = Expenses.GetBy((Importance)i);
                foreach (var entry in expenses) //not linq since when empty category it crashes
                {
                    temp += entry.Amount;
                }
                if (temp < sum) continue;
                sum = temp;
                biggestCategory = (Importance)i;
            }

            Result.BiggestCategory = biggestCategory;
            Result.BiggestCategorySum = sum;
            var necessarySum = Expenses.GetBy(Importance.Necessary).Sum(entry => entry.Amount);
            Result.NecessarySum = necessarySum;
            Result.HowMuchBigger = ((sum - necessarySum) / necessarySum * 100);
        }

        private  List<IGoal> CalculateSavedUpGoals()
        {
            return AllSavings >= 0 ? Goals.Where(x => x.Amount <= AllSavings).ToList() : new GoalList();
        }

        private void CalculateGoalReport()
        {
            var savedThisMonth = Income.GetSum() - Expenses.GetSum();
            if (Goals.Count() < 2 || savedThisMonth <= 0)
            {
                Result.HasGoals = false;
                return;
            }

            Result.HasGoals = true;
            var min = decimal.MaxValue;
            var minGoal = new Goal();
            var maxGoal = new Goal();
            var max = 0M;
            foreach (var item in Goals)
            {
                if (item.Amount >= max)
                {
                    max = item.Amount;
                    maxGoal = (Goal)item;
                }

                if (item.Amount > min || item.Amount <= AllSavings ) continue;
                min = item.Amount;
                minGoal = (Goal) item;
            }

            Result.CheapestGoal = minGoal;
            Result.MostExpensiveGoal = maxGoal;

            Result.MonthsForCheapestGoal = (int) decimal.Ceiling(minGoal.Amount - AllSavings / savedThisMonth);
            Result.MonthsForMostExpensiveGoal = (int)decimal.Ceiling(maxGoal.Amount - AllSavings / savedThisMonth);

        }
    }
}
