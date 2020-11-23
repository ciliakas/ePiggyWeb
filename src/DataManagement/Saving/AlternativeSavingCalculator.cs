using System;
using System.Collections.Generic;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Saving
{
    public static class AlternativeSavingCalculator
    {
        private static decimal RegularSavingValue { get; } = 0.25M;
        private static decimal MaximalSavingValue { get; } = 0.5M;
        private static decimal MinimalSavingValue { get; } = 0.1M;

        public static int GetSuggestedExpensesOffers(IEntryList entryList, IGoal goal, IList<ISavingSuggestion> entrySuggestions,
            List<SavingSuggestionByMonth> monthlySuggestions, decimal startingBalance, SavingType savingType = SavingType.Regular)
        {
            entrySuggestions ??= new List<ISavingSuggestion>();

            if (entryList is null)
            {
                return 0;
            }

            var enumCount = Enum.GetValues(typeof(Importance)).Length;
 
            var sumsOfAmountByImportanceAdjusted = new decimal[enumCount];
            var sumsOfAmountByImportanceDefault = new decimal[enumCount];
            var averagesOfAmountByImportanceAdjusted = new decimal[enumCount];
            var averagesOfAmountByImportanceDefault = new decimal[enumCount];
            var entryAmounts = new int[enumCount];

            for (var i = enumCount; i > (int)Importance.Necessary; i--)
            {
                var expenses = entryList.GetBy((Importance)i);
                var ratio = enumCount - i;
                foreach (var entry in expenses)
                {
                    var amountAfterSaving = savingType switch
                    {
                        SavingType.Minimal => entry.Amount * ratio * MinimalSavingValue,
                        SavingType.Regular => entry.Amount * ratio * RegularSavingValue,
                        SavingType.Maximal => entry.Amount * ratio * MaximalSavingValue,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    entrySuggestions.Add(new SavingSuggestion(entry, amountAfterSaving));

                    sumsOfAmountByImportanceAdjusted[i - 1] += amountAfterSaving;
                    sumsOfAmountByImportanceDefault[i - 1] += entry.Amount;
                    entryAmounts[i - 1]++;

                }
            }
            var timesToRepeatSaving = 0;
            var approximateSavedAmount = startingBalance;
            while (goal.Amount > approximateSavedAmount)
            {
                for (var i = enumCount; i > (int)Importance.Necessary; i--)
                {
                    if (entryAmounts[i - 1] != 0)
                    {
                        averagesOfAmountByImportanceAdjusted[i - 1] = sumsOfAmountByImportanceAdjusted[i - 1] / entryAmounts[i - 1];
                        averagesOfAmountByImportanceDefault[i - 1] = sumsOfAmountByImportanceDefault[i - 1] / entryAmounts[i - 1];
                    }
                    else
                    {
                        averagesOfAmountByImportanceAdjusted[i - 1] = 0;
                        averagesOfAmountByImportanceDefault[i - 1] = 0;
                    }

                    approximateSavedAmount += averagesOfAmountByImportanceDefault[i - 1] - averagesOfAmountByImportanceAdjusted[i - 1];
                }
                timesToRepeatSaving++;              
            }
            for (var i = enumCount; i > (int)Importance.Necessary; i--)
            {
                monthlySuggestions.Add(new SavingSuggestionByMonth(averagesOfAmountByImportanceAdjusted[i - 1], averagesOfAmountByImportanceDefault[i - 1], (Importance)i));
            }
            return timesToRepeatSaving;
        }
    }
}
