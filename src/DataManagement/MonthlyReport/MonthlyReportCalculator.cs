using System;
using System.Diagnostics;
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
            return Result;
        }

        private async Task<MonthlyReportResult> GatherDataAsync()
        {
            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            var first = month.AddMonths(-1);
            var last = month.AddDays(-1);
            Debug.WriteLine(first);
            Debug.WriteLine(last);
            Expenses = (await EntryDatabase.ReadListAsync(UserId, EntryType.Expense)).GetFrom(first).GetTo(last);
            Income = (await EntryDatabase.ReadListAsync(UserId, EntryType.Income)).GetFrom(first).GetTo(last);
            Goals = await GoalDatabase.ReadListAsync(UserId);
            return new MonthlyReportResult(Expenses.GetSum(), Income.GetSum(), first, last);

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
                foreach (var entry in expenses)
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
    }
}
