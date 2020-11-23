using System;
using System.Collections.Generic;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace ePiggyWeb.DataManagement.Saving
{
    public class ThreadingCalculator
    {
        public Dictionary<SavingType, CalculationResults> GetAllSuggestedExpenses(IEntryList entryList, IGoal goal, decimal startingBalance)
        {
            var fullResults = new Dictionary<SavingType, CalculationResults>();
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
                var task = ThreadWorkAsync(entryList, goal, startingBalance, (SavingType)savingType);
                //var result = alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, (SavingType)savingType);
                //AsyncCallback fullResults.Add((SavingType)savingType, result);
                tasks.Add(task);
                
            }
            Task.WaitAll(tasks.ToArray());
            return fullResults;
        }
        private async Task<CalculationResults> ThreadWorkAsync(IEntryList entryList, IGoal goal, decimal startingBalance, SavingType savingType)
        {
            var fullResults = new Dictionary<SavingType, CalculationResults>();
            var alternativeSavingCalculator = new AlternativeSavingCalculator();
            var result = await Task.Factory.StartNew(() => alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, (SavingType)savingType));
            //var resultRegular = await Task.Factory.StartNew(() => alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, SavingType.Regular));
            //var resultMaximal = await Task.Factory.StartNew(() => alternativeSavingCalculator.GetSuggestedExpensesOffers(entryList, goal, startingBalance, SavingType.Maximal));
            return result;
        }
      
    }
}
