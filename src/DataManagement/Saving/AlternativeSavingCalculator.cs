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
            decimal startingBalance, SavingType savingType = SavingType.Regular)
        {
            entrySuggestions ??= new List<ISavingSuggestion>();

            if (entryList is null)
            {
                return false;
            }

            var enumCount = Enum.GetValues(typeof(Importance)).Length;

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

                    startingBalance += entry.Amount - amountAfterSaving;
                    if (goal.Amount <= startingBalance)
                    {
                        return true; //Saved enough
                    }
                }
            }
            return false; //Didn't save enough
        }
    }
}
