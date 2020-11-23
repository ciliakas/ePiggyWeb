using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Saving
{
    public class SavingSuggestionByImportance
    {
        public decimal OldAverage { get; set; }
        public decimal NewAverage { get; set; }
        public Importance Importance { get; set; }

        public SavingSuggestionByImportance(decimal newAverage, decimal oldAverage, Importance importance)
        {
            Importance = importance;
            NewAverage = newAverage;
            OldAverage = oldAverage;

        }
    }
}
