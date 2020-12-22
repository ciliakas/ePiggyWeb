using System.Collections.Generic;

namespace ePiggyWeb.DataManagement.Saving
{
    public class CalculationResults
    {
        public readonly IList<ISavingSuggestion> EntrySuggestions;
        public readonly List<SavingSuggestionByImportance> ImportanceSuggestions;
        public int TimesToRepeatSaving;

        public CalculationResults(IList<ISavingSuggestion> entrySuggestions, List<SavingSuggestionByImportance> importanceSuggestions, int timesToRepeatSaving)
        {
            EntrySuggestions = entrySuggestions;
            ImportanceSuggestions = importanceSuggestions;
            TimesToRepeatSaving = timesToRepeatSaving;
        }
    }
}
