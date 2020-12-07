using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.MonthlyReport
{
    public class MonthlyReportResult
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Expenses { get; set; }
        public decimal Income { get; set; }
        public decimal Balance { get; set; }
        public Importance BiggestCategory { get; set; }
        public decimal BiggestCategorySum { get; set; }
        public decimal NecessarySum { get; set; }
        public decimal HowMuchBigger { get; set; }
        public List<IGoal> SavedUpGoals { get; set; }
        public bool HasGoals { get; set; }
        public IGoal MostExpensiveGoal { get; set; }
        public int MonthsForMostExpensiveGoal { get; set; }
        public IGoal CheapestGoal { get; set; }
        public int MonthsForCheapestGoal { get; set; }

        public MonthlyReportResult(decimal expenses, decimal income, decimal balance, DateTime start, DateTime end)
        {
            Expenses = expenses;
            Income = income;
            Balance = balance;
            Start = start;
            End = end;
        }
    }
}
