﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.MonthlyReport
{
    public class MonthlyReportCalculator
    {
        private int UserId { get; set; }
        private IGoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }

        private IEntryList Expenses { get; set; }
        private IEntryList Income { get; set; }
        private decimal Balance { get; set; }
        private decimal AllSavings { get; set; }
        private IGoalList Goals { get; set; }

        private MonthlyReportResult Result { get; set; }

        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }
        private CurrencyConverter CurrencyConverter { get; set; }

        public MonthlyReportCalculator(IGoalDatabase goalDatabase, EntryDatabase entryDatabase, CurrencyConverter currencyConverter)
        {
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            CurrencyConverter = currencyConverter;
        }

        public async Task<MonthlyReportResult> Calculate(int userId)
        {
            UserId = userId;
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

            Goals = await GoalDatabase.ReadListAsync(UserId);
            Expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
            Income = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);

            /*Allowing exception to bubble here*/
            Goals = await CurrencyConverter.ConvertGoalList(Goals, UserId);
            Income = await CurrencyConverter.ConvertEntryList(Income, UserId);
            Expenses = await CurrencyConverter.ConvertEntryList(Expenses, UserId);



            var expensesTotal = Expenses.GetSum();
            var incomeTotal = Income.GetSum();
            var thisMonthExpenses = Expenses.GetFrom(StartTime).GetTo(EndTime).GetSum();
            var thisMonthIncome = Income.GetFrom(StartTime).GetTo(EndTime).GetSum();
            AllSavings = incomeTotal - expensesTotal;
            Balance = thisMonthIncome - thisMonthExpenses;

            var previousMonthStart = StartTime.AddMonths(-1);
            var previousMonthEnd = StartTime.AddDays(-1);
            var previousMonthIncome = Income.GetFrom(previousMonthStart).GetTo(previousMonthEnd).GetSum();
            var differenceInIncome = thisMonthIncome - previousMonthIncome;

            return new MonthlyReportResult(thisMonthExpenses, thisMonthIncome, differenceInIncome, Balance, StartTime, EndTime);
        }

        private void CalculateBiggestExpensesCategory()
        {
            var enumCount = Enum.GetValues(typeof(Importance)).Length;
            var biggestCategory = (Importance)enumCount;
            var sum = 0m;
            var monthExpenses = Expenses.GetFrom(StartTime).GetTo(EndTime);

            foreach (var importance in Enum.GetValues(typeof(Importance)).Cast<Importance>())
            {
                var expenses = monthExpenses.GetBy(importance);
                var temp = expenses.Sum(entry => entry.Amount);
                if (temp < sum) continue;
                sum = temp;
                biggestCategory = importance;
            }

            Result.BiggestCategory = biggestCategory;
            Result.BiggestCategorySum = sum;
            Result.NecessarySum = monthExpenses.GetBy(Importance.Necessary).Sum(entry => entry.Amount);
            if (Result.NecessarySum == 0)
            {
                Result.HowMuchBigger = -1;
                return;
            }
            Result.HowMuchBigger = ((sum - Result.NecessarySum) / Result.NecessarySum * 100);
        }

        private List<IGoal> CalculateSavedUpGoals()
        {
            return AllSavings >= 0 ? Goals.Where(x => x.Amount <= AllSavings).ToList() : new GoalList();
        }

        private void CalculateGoalReport()
        {
            var goals = Goals.Where(x => x.Amount > AllSavings).ToList();
            if (goals.Count < 2 || Balance <= 0)
            {
                Result.HasGoals = false;
                return;
            }

            var maxGoal = goals.Aggregate((expensive, next) => next.Amount >= expensive.Amount ? next : expensive);
            var minGoal = goals.Aggregate((cheapest, next) =>
                next.Amount <= cheapest.Amount && next.Amount >= AllSavings ? next : cheapest);

            if (maxGoal.Amount <= AllSavings && minGoal.Amount <= AllSavings)
            {
                Result.HasGoals = false;
                return;
            }

            Result.CheapestGoal = minGoal;
            Result.MostExpensiveGoal = maxGoal;
            Result.HasGoals = true;

            Result.MonthsForCheapestGoal = (int)decimal.Ceiling((minGoal.Amount - AllSavings) / Balance);
            Result.MonthsForMostExpensiveGoal = (int)decimal.Ceiling((maxGoal.Amount - AllSavings) / Balance);

        }
    }
}
