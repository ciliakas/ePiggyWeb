using ePiggyWeb.Utilities;
using System.Collections.Generic;

namespace ePiggyWeb.DataManagement.Saving
{
    public class CalculationResults
    {
        public IList<ISavingSuggestion> EntrySuggestions;
        public List<SavingSuggestionByImportance> ImportanceSuggestions;
        public int TimesToRepeatSaving;

        public CalculationResults(IList<ISavingSuggestion> entrySuggestions, List<SavingSuggestionByImportance> importanceSuggestions, int timesToRepeatSaving)
        {
            EntrySuggestions = entrySuggestions;
            ImportanceSuggestions = importanceSuggestions;
            TimesToRepeatSaving = timesToRepeatSaving;
        }

        public CalculationResults()
        {
            EntrySuggestions = new List<ISavingSuggestion>();
            ImportanceSuggestions = new List<SavingSuggestionByImportance>();
            TimesToRepeatSaving = 0;
        }
    }
}
