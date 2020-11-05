using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Saving
{
    public class SavingSuggestion : ISavingSuggestion
    {
        public IEntry Entry { get; set; }
        public decimal Amount { get; set; }

        public SavingSuggestion(IEntry entry, decimal amount)
        {
            Entry = entry;
            Amount = amount;
        }

        public int CompareTo(ISavingSuggestion other)
        {
            return other is null ? 1 : Amount.CompareTo(other.Amount);
        }

        public int CompareTo(decimal other)
        {
            return Amount.CompareTo(other);
        }

        public bool Equals(ISavingSuggestion other)
        {
            if (other is null)
            {
                return false;
            }

            return Amount == other.Amount;
        }

        public bool Equals(decimal other)
        {
            return Amount == other;
        }

        public override string ToString()
        {
            return Entry + " Suggested Amount: " + NumberFormatter.FormatCurrency(Amount);
        }
    }
}
