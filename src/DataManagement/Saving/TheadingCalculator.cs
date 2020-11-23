﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Saving
{
    public class ThreadingCalculator
    {
        public Dictionary<SavingType, CalculationResults> GetAllSuggestedExpenses(IEntryList entryList, IGoal goal, decimal startingBalance)
        {
            var fullResults = new Dictionary<SavingType, CalculationResults>();
            var alternativeSavingCalculator = new AlternativeSavingCalculator();
            var savingTypes = Enum.GetValues(typeof(SavingType));
            //TODO: insert threads here
            foreach (var savingType in savingTypes)
            {
                var result = alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, (SavingType)savingType);
                fullResults.Add((SavingType)savingType, result);
            }
            return fullResults;
        }
    }
}
