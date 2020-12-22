using System;
using System.Collections.Generic;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace ePiggyWeb.DataManagement.Saving
{
    public class CalculatorRunner
    {
        private readonly ConcurrentDictionary<SavingType, CalculationResults> _fullResults =
            new ConcurrentDictionary<SavingType, CalculationResults>();
        public ConcurrentDictionary<SavingType, CalculationResults> GetAllSuggestedExpenses(IEntryList entryList,
            IGoal goal, decimal startingBalance, IConfiguration configuration)
        {
            var tasks = new List<Task>();
            var savingTypes = Enum.GetValues(typeof(SavingType));

            foreach (var savingType in savingTypes)
            {
                var task = ThreadWorkAsync(entryList, goal, startingBalance, (SavingType)savingType, configuration);
                tasks.Add(task);

            }
            Task.WaitAll(tasks.ToArray());
            return _fullResults;
        }

        private async Task<CalculationResults> ThreadWorkAsync(IEntryList entryList, IGoal goal,
            decimal startingBalance, SavingType savingType, IConfiguration configuration)
        {
            var alternativeSavingCalculator = new SavingCalculator();
            var result = await Task.Factory.StartNew(() =>
                alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, savingType,
                    configuration));
            _fullResults.TryAdd(savingType, result);
            return result;
        }
    }
}
