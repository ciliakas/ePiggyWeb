using System;
using System.Collections.Generic;
using System.Linq;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Saving
{
    public class SavingCalculator
    {
        private static decimal RegularSavingValue { get; } = 0.25M;
        private static decimal MaximalSavingValue { get; } = 0.5M;
        private static decimal MinimalSavingValue { get; } = 0.1M;
        private List<ISavingSuggestion> EntrySuggestions { get; set; }
        private List<SavingSuggestionByImportance> SavingSuggestionByImportanceList { get; set; }
        private IEntryList ExpenseList { get; }
        private IEntryList IncomeList { get; }
        public decimal MonthlyIncome { get; set; }
        private IGoal Goal { get; }
        private decimal StartingBalance { get; }
        private SavingType SavingType { get; set; }
        private int EnumCount { get; }
        private int MonthsToSave { get; set; }

        public SavingCalculator(IEntryList expenseList, IEntryList incomeList, IGoal goal, decimal startingBalance)
        {
            // Reikia gauti average income
            // Tada reikia, kad kas menesi prisidetu recurring + average per menesi
            // I menesi sutaupai = averageIncome + savedInAMonth
            // savedInAMonth = 

            IncomeList = incomeList;
            ExpenseList = expenseList;
            Goal = goal;
            StartingBalance = startingBalance;
            EnumCount = Enum.GetValues(typeof(Importance)).Length;
            CalculateMonthlyIncome();
        }

        public CalculationResults GetSuggestedExpensesOffers(SavingType savingType = SavingType.Regular)
        {
            SavingType = savingType;
            EntrySuggestions = new List<ISavingSuggestion>();
            SavingSuggestionByImportanceList = new List<SavingSuggestionByImportance>();
            GetAverages();
            CalculateSavingTime();
            return new CalculationResults(EntrySuggestions, SavingSuggestionByImportanceList, MonthsToSave);
        }

        private void CalculateMonthlyIncome()
        {
            if (IncomeList.Any())
            {
                var newest = IncomeList.GetNewestEntryDate();
                var oldest = IncomeList.GetOldestEntryDate();
                var months = 0;
                while (oldest <= newest)
                {
                    months++;
                    oldest = TimeManager.MoveToNextMonth(oldest);
                }

                if (months < 1)
                {
                    MonthlyIncome = 0;
                }
                else
                {
                    MonthlyIncome = IncomeList.GetSum() / months;
                }
            }
            else
            {
                MonthlyIncome = 0;
            }
        }

        private void GetAverages()
        {
            for (var i = EnumCount; i > (int)Importance.Necessary; i--)
            {
                var listByImportance = ExpenseList.GetBy((Importance)i);
                if (listByImportance.Count < 1)
                {
                    SavingSuggestionByImportanceList.Add(new SavingSuggestionByImportance(0,
                        0, (Importance)i));
                    continue;
                }

                var ratio = EnumCount - i;
                var oldest = listByImportance.GetOldestEntryDate();
                var newest = listByImportance.GetNewestEntryDate();

                var importanceAverage = 0M;
                var months = 0;

                while (oldest <= newest)
                {
                    var listByMonth = listByImportance.GetBy(oldest);
                    var monthTotal = 0M;

                    foreach (var entry in listByMonth)
                    {
                        var amountAfterSaving = SavingType switch
                        {
                            SavingType.Minimal => entry.Amount * ratio * MinimalSavingValue,
                            SavingType.Regular => entry.Amount * ratio * RegularSavingValue,
                            SavingType.Maximal => entry.Amount * ratio * MaximalSavingValue,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        amountAfterSaving = amountAfterSaving > entry.Amount ? entry.Amount : amountAfterSaving;
                        EntrySuggestions.Add(new SavingSuggestion(entry, amountAfterSaving));
                        monthTotal += entry.Amount;
                    }

                    importanceAverage += monthTotal;//listByMonth.Average(x => x.Amount));
                    months++;
                    oldest = TimeManager.MoveToNextMonth(oldest);
                }

                importanceAverage /= months > 0 ? months : 1;

                var newAverage = SavingType switch
                {
                    SavingType.Minimal => importanceAverage * ratio * MinimalSavingValue,
                    SavingType.Regular => importanceAverage * ratio * RegularSavingValue,
                    SavingType.Maximal => importanceAverage * ratio * MaximalSavingValue,
                    _ => throw new ArgumentOutOfRangeException()
                };

                SavingSuggestionByImportanceList.Add(new SavingSuggestionByImportance(newAverage, importanceAverage, (Importance)i));
            }
        }

        private void CalculateSavingTime()
        {
            var increasedMonthlySavings = SavingSuggestionByImportanceList.Sum(savingSuggestionByImportance => savingSuggestionByImportance.OldAverage - savingSuggestionByImportance.NewAverage);
            var amountToSave = Goal.Amount - StartingBalance;
            MonthsToSave = (int)Math.Ceiling(decimal.Divide(amountToSave, MonthlyIncome + increasedMonthlySavings));
            if (MonthsToSave > 1000)
            {
                MonthsToSave = 0;
            }
        }
    }
}
