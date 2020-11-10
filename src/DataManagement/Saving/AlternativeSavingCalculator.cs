using System;
using System.Collections.Generic;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Saving
{
    public static class AlternativeSavingCalculator
    {
        //This might not be 1M in the future, that why we are keeping this
        private static decimal SavingRatio { get; } = 1M;
        private static decimal RegularSavingValue { get; } = 0.25M;
        private static decimal MaximalSavingValue { get; } = 0.5M;
        private static decimal MinimalSavingValue { get; } = 0.1M;

        public static bool GetSuggestedExpensesOffers(IEntryList entryList, IGoal goal, IList<ISavingSuggestion> entrySuggestions,
            List<SavingSuggestionByMonth> monthlySuggestions, decimal startingBalance, SavingType savingType = SavingType.Regular)
        {
            entrySuggestions ??= new List<ISavingSuggestion>();

            if (entryList is null)
            {
                return false;
            }

            var enumCount = Enum.GetValues(typeof(Importance)).Length;

            decimal[] sumsOfAmountByImportanceAdjusted = new decimal[enumCount];
            decimal[] sumsOfAmountByImportanceDefault = new decimal[enumCount];
            decimal[] averagesOfAmountByImportanceAdjusted = new decimal[enumCount];
            decimal[] averagesOfAmountByImportanceDefault = new decimal[enumCount];
            int[] entryAmounts = new int[enumCount];

            for (var i = enumCount; i > (int)Importance.Necessary; i--)
            {
                var expenses = entryList.GetBy((Importance)i - 1);
                var ratio = enumCount - i;
                foreach (var entry in expenses)
                {
                    var amountAfterSaving = savingType switch
                    {
                        SavingType.Minimal => entry.Amount * SavingRatio * ratio * MinimalSavingValue,
                        SavingType.Regular => entry.Amount * SavingRatio * ratio * RegularSavingValue,
                        SavingType.Maximal => entry.Amount * SavingRatio * ratio * MaximalSavingValue,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    entrySuggestions.Add(new SavingSuggestion(entry, amountAfterSaving));

                    sumsOfAmountByImportanceAdjusted[i] += amountAfterSaving;
                    sumsOfAmountByImportanceDefault[i] += entry.Amount;
                    entryAmounts[i]++;

                    // Commented as the logic with returns is still WIP
                    /*
                    startingBalance += entry.Amount - amountAfterSaving;
                    if (goal.Amount <= startingBalance)
                    {
                        return true;
                    }
                    */
                }
            }
            var savedByGuessing = 0M;
            while (goal.Amount > savedByGuessing)
            {
                for (var i = enumCount; i > (int)Importance.Necessary; i--)
                {
                    averagesOfAmountByImportanceAdjusted[i] = sumsOfAmountByImportanceAdjusted[i] / entryAmounts[i];
                    averagesOfAmountByImportanceDefault[i] = sumsOfAmountByImportanceDefault[i] / entryAmounts[i];

                    savedByGuessing += averagesOfAmountByImportanceAdjusted[i];

                    monthlySuggestions.Add(new SavingSuggestionByMonth(averagesOfAmountByImportanceAdjusted[i], averagesOfAmountByImportanceDefault[i], (Importance)i));
                }                              
            }
            return true;
        }
    }
}
