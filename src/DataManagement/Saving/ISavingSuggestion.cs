using System;
using ePiggyWeb.DataManagement.Entries;

namespace ePiggyWeb.DataManagement.Saving
{
    public interface ISavingSuggestion : IComparable<ISavingSuggestion>, IComparable<decimal>, IEquatable<ISavingSuggestion>, IEquatable<decimal>
    {
        public IEntry Entry { get; set; }
        public decimal Amount { get; set; }
    }
}
