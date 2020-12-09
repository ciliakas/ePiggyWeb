﻿using System;
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
        private decimal Balance { get; set; }
        private decimal AllSavings { get; set; }
        private IGoalList Goals { get; set; }

        private MonthlyReportResult Result { get; set; }

        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }

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
            StartTime = month.AddMonths(-1);
            EndTime = month.AddDays(-1);

            Expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
            Income = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);
            Goals = await GoalDatabase.ReadListAsync(UserId);

            var expensesTotal = Expenses.GetSum();
            var incomeTotal = Income.GetSum();
            var thisMonthExpenses = Expenses.GetFrom(StartTime).GetTo(EndTime).GetSum();
            var thisMonthIncome = Income.GetFrom(StartTime).GetTo(EndTime).GetSum();
            AllSavings = incomeTotal - expensesTotal;
            Balance = thisMonthIncome - thisMonthExpenses;

            return new MonthlyReportResult(thisMonthExpenses, thisMonthIncome, Balance, StartTime, EndTime);

        }

        private void CalculateBiggestExpensesCategory()
        {
            var enumCount = Enum.GetValues(typeof(Importance)).Length;
            var biggestCategory = (Importance)enumCount;
            var sum = 0m;
            foreach (var importance in Enum.GetValues(typeof(Importance)).Cast<Importance>())
            {
                var expenses = Expenses.GetBy(importance);
                var temp = expenses.Sum(entry => entry.Amount);
                if (temp < sum) continue;
                sum = temp;
                biggestCategory = importance;
            }

            Result.BiggestCategory = biggestCategory;
            Result.BiggestCategorySum = sum;
            var necessarySum = Expenses.GetBy(Importance.Necessary).Sum(entry => entry.Amount);
            Result.NecessarySum = necessarySum;
            if (necessarySum == 0)
            {
                Result.HowMuchBigger = -1;
                return;
            }
            Result.HowMuchBigger = ((sum - necessarySum) / necessarySum * 100);
        }

        private List<IGoal> CalculateSavedUpGoals()
        {
            return AllSavings >= 0 ? Goals.Where(x => x.Amount <= AllSavings).ToList() : new GoalList();
        }

        private void CalculateGoalReport()
        {
            if (Goals.Count < 2 || Balance <= 0)
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

                if (item.Amount > min || item.Amount <= AllSavings) continue;
                min = item.Amount;
                minGoal = (Goal)item;
            }

            Result.CheapestGoal = minGoal;
            Result.MostExpensiveGoal = maxGoal;

            Result.MonthsForCheapestGoal = (int)decimal.Ceiling((minGoal.Amount - AllSavings) / Balance);
            Result.MonthsForMostExpensiveGoal = (int)decimal.Ceiling((maxGoal.Amount - AllSavings) / Balance);

        }
    }
}