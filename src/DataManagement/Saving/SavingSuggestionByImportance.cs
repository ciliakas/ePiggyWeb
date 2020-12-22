using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Saving
{
    public class SavingSuggestionByImportance
    {
        public decimal OldAverage { get; }
        public decimal NewAverage { get; }
        public Importance Importance { get; }

        public SavingSuggestionByImportance(decimal newAverage, decimal oldAverage, Importance importance)
        {
            Importance = importance;
            NewAverage = newAverage;
            OldAverage = oldAverage;
        }
    }
}
