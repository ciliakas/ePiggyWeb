using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Saving
{
    public class SavingSuggestionByMonth
    {
        public decimal OldAverage { get; set; }
        public decimal NewAverage { get; set; }
        public Importance Importance { get; set; }

        public SavingSuggestionByMonth(decimal newAverage, decimal oldAverage, Importance importance)
        {
            Importance = importance;
            NewAverage = newAverage;
            OldAverage = oldAverage;

        }
    }
}
