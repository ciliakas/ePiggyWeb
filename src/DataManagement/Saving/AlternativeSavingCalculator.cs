﻿using System;
using System.Collections.Generic;
using System.Linq;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using Microsoft.Extensions.Configuration;

namespace ePiggyWeb.DataManagement.Saving
{
    public class AlternativeSavingCalculator
    {
        private static decimal RegularSavingValue { get; } = 0.25M;
        private static decimal MaximalSavingValue { get; } = 0.1M;
        private static decimal MinimalSavingValue { get; } = 0.5M;

        public CalculationResults GetSuggestedExpensesOffers(IEntryList entryList, IGoal goal, decimal startingBalance, SavingType savingType, IConfiguration configuration)
        {
            var entrySuggestions = new List<ISavingSuggestion>();
            var monthlySuggestions = new List<SavingSuggestionByImportance>();
            var expensesRandomList = EntryList.RandomList(configuration, EntryType.Expense);
            var generateRandomData = false;

            var enumCount = Enum.GetValues(typeof(Importance)).Length;
 
            var sumsOfAmountByImportanceAdjusted = new decimal[enumCount];
            var sumsOfAmountByImportanceDefault = new decimal[enumCount];
            var averagesOfAmountByImportanceAdjusted = new decimal[enumCount];
            var averagesOfAmountByImportanceDefault = new decimal[enumCount];
            var entryAmounts = new int[enumCount];

            if (entryList.Count == 0)
            {
                generateRandomData = true;              
            }

            var listToUse = generateRandomData ? expensesRandomList : entryList;
            var groupedByImportance = (from entry in listToUse
                                       where entry.Importance != 1
                                       group entry by entry.Importance).ToArray();

            foreach (var group in groupedByImportance)
            {
                var i = group.Key;
                var expenses = group.ToIEntryList();

                var ratio = enumCount - i;
                foreach (var entry in expenses)
                {
                    decimal amountAfterSaving;
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
                        default:
                            throw new Exception("Unexpected saving type");
                    }
                    entrySuggestions.Add(new SavingSuggestion(entry, amountAfterSaving));

                    sumsOfAmountByImportanceAdjusted[i - 1] += amountAfterSaving;
                    sumsOfAmountByImportanceDefault[i - 1] += entry.Amount;
                    entryAmounts[i - 1]++;

                }
            }
            var timesToRepeatSaving = 0;
            var firstTimeThroughWhile = true;
            var approximateSavedAmount = startingBalance;
            while (goal.Amount > approximateSavedAmount)
            {
                if(!firstTimeThroughWhile && approximateSavedAmount <= startingBalance) //Can't possibly save for goal
                {
                    var entrySuggestionsEmpty = new List<ISavingSuggestion>();
                    return new CalculationResults(entrySuggestionsEmpty, monthlySuggestions, 0);
                }
                firstTimeThroughWhile = false;
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
            return !generateRandomData ? new CalculationResults(entrySuggestions, monthlySuggestions, timesToRepeatSaving)
                                       : new CalculationResults(entrySuggestions, monthlySuggestions, timesToRepeatSaving * -1);
        }
    }
}
