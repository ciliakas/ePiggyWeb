using System;
using System.Collections.Generic;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ePiggyWeb.DataManagement.Saving
{
    public class ThreadingCalculator
    {
        public ConcurrentDictionary<SavingType, CalculationResults> GetAllSuggestedExpenses(IEntryList entryList, IGoal goal, decimal startingBalance)
        {
            var fullResults = new ConcurrentDictionary<SavingType, CalculationResults>();
            //var alternativeSavingCalculator = new AlternativeSavingCalculator();
            var tasks = new List<Task>();
            var savingTypes = Enum.GetValues(typeof(SavingType));
            /*
            Thread threadMinimal = new Thread(() => {
                var resultMinimal = alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, SavingType.Minimal);
            });
            Thread threadRegular = new Thread(() => {
                var resultRegular = alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, SavingType.Regular);
            });
            Thread threadMaximal = new Thread(() => {
                var resultMaximal = alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, SavingType.Maximal);
            });

            threadMinimal.Start();
            threadRegular.Start();
            threadMaximal.Start();
            while(threadMaximal.IsAlive && threadRegular.IsAlive && threadMaximal.IsAlive)
            {
                if (!(threadMaximal.IsAlive && threadRegular.IsAlive && threadMaximal.IsAlive))
                {
                    return fullResults;
                }
            }
            */
            
            foreach (var savingType in savingTypes)
            {
                /*
                Thread thread = new Thread(() => {
                    var resultMaximal = alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, (SavingType)savingType);
                });
                */
                var task = ThreadWorkAsync(entryList, goal, startingBalance, (SavingType)savingType, fullResults);
                //var result = alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, (SavingType)savingType);
                //AsyncCallback fullResults.Add((SavingType)savingType, result);
                tasks.Add(task);
                
            }
            Task.WaitAll(tasks.ToArray());
            return fullResults;
        }
        private async Task<CalculationResults> ThreadWorkAsync(IEntryList entryList, IGoal goal, decimal startingBalance, SavingType savingType,
            ConcurrentDictionary<SavingType, CalculationResults> fullResults)
        {
            var alternativeSavingCalculator = new AlternativeSavingCalculator();
            var result = await Task.Factory.StartNew(() => alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, (SavingType)savingType));
            fullResults.TryAdd((SavingType)savingType, result);
            return result;
        }
      
    }
}
