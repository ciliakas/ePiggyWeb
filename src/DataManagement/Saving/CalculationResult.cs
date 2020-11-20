using ePiggyWeb.Utilities;
using System.Collections.Generic;

namespace ePiggyWeb.DataManagement.Saving
{
    public class CalculationResults
    {
        public IList<ISavingSuggestion> EntrySuggestions;
        public List<SavingSuggestionByMonth> MonthlySuggestions;
        public int TimesToRepeatSaving;

        public CalculationResults(IList<ISavingSuggestion> entrySuggestions, List<SavingSuggestionByMonth> monthlySuggestions, int timesToRepeatSaving)
        {
            EntrySuggestions = entrySuggestions;
            MonthlySuggestions = monthlySuggestions;
            TimesToRepeatSaving = timesToRepeatSaving;
        }

        public CalculationResults()
        {
            EntrySuggestions = new List<ISavingSuggestion>();
            MonthlySuggestions = new List<SavingSuggestionByMonth>();
            TimesToRepeatSaving = 0;
        }
    }
}
