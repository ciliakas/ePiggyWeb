using System;
using System.Collections.Generic;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using System.Diagnostics;

namespace ePiggyWeb.DataManagement.Saving
{
    public class AlternativeSavingCalculator
    {
        private static decimal RegularSavingValue { get; } = 0.25M;
        private static decimal MaximalSavingValue { get; } = 0.1M;
        private static decimal MinimalSavingValue { get; } = 0.5M;

        public CalculationResults GetSuggestedExpensesOffers(IEntryList entryList, IGoal goal, decimal startingBalance, SavingType savingType = SavingType.Regular)
        {
            var entrySuggestions = new List<ISavingSuggestion>();
            var monthlySuggestions = new List<SavingSuggestionByImportance>();
            
            if (entryList is null)
            {
                return new CalculationResults(entrySuggestions, monthlySuggestions, 0);              
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
                    var amountAfterSaving = 0M;
                    switch (savingType)
                    {
                        case SavingType.Minimal:
                            if (entry.Amount * ratio * MinimalSavingValue < entry.Amount)
                            {
                                amountAfterSaving = entry.Amount * ratio * MinimalSavingValue;
                            }
                            else
                            {
                                amountAfterSaving = entry.Amount;
                            }
                            break;
                        case SavingType.Regular:
                            amountAfterSaving = entry.Amount * ratio * RegularSavingValue;
                            break;
                        case SavingType.Maximal:
                            amountAfterSaving = entry.Amount * ratio * MaximalSavingValue;                            
                            break;
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
                monthlySuggestions.Add(new SavingSuggestionByImportance(averagesOfAmountByImportanceAdjusted[i - 1], averagesOfAmountByImportanceDefault[i - 1], (Importance)i));
            }
            return new CalculationResults(entrySuggestions, monthlySuggestions, timesToRepeatSaving);
        }
    }
}
