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

        public static void GetSuggestedExpensesOffers(IEntryList entryList, IGoal goal, IList<ISavingSuggestion> entrySuggestions,
            List<SavingSuggestionByMonth> monthlySuggestions, decimal startingBalance, SavingType savingType = SavingType.Regular)
        {
            entrySuggestions ??= new List<ISavingSuggestion>();

            if (entryList is null)
            {
                return;
            }

            var enumCount = Enum.GetValues(typeof(Importance)).Length;

            decimal[] arrayTemplate = new decimal[enumCount];
            decimal[] sumsOfAmountByImportanceAdjusted = arrayTemplate;
            decimal[] sumsOfAmountByImportanceDefault = arrayTemplate;
            decimal[] averagesOfAmountByImportanceAdjusted = arrayTemplate;
            decimal[] averagesOfAmountByImportanceDefault = arrayTemplate;
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

                    sumsOfAmountByImportanceAdjusted[i - 1] += amountAfterSaving;
                    sumsOfAmountByImportanceDefault[i - 1] += entry.Amount;
                    entryAmounts[i - 1]++;

                }
            }
            var approximateSavedAmount = 0M;
            while (goal.Amount > approximateSavedAmount)
            {
                for (var i = enumCount; i > (int)Importance.Necessary; i--)
                {
                    averagesOfAmountByImportanceAdjusted[i - 1] = sumsOfAmountByImportanceAdjusted[i - 1] / entryAmounts[i - 1];
                    averagesOfAmountByImportanceDefault[i - 1] = sumsOfAmountByImportanceDefault[i - 1] / entryAmounts[i - 1];

                    approximateSavedAmount += averagesOfAmountByImportanceDefault[i - 1] - averagesOfAmountByImportanceAdjusted[i - 1];

                    monthlySuggestions.Add(new SavingSuggestionByMonth(averagesOfAmountByImportanceAdjusted[i - 1], averagesOfAmountByImportanceDefault[i - 1], (Importance)i));
                }                              
            }
        }
    }
}
