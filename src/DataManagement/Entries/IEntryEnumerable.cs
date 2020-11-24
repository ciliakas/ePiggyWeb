using System.Collections.Generic;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntryEnumerable : IEnumerable<IEntry>
    {
        /*
        At the moment this interface is not used anywhere, but it is more general then IList<IEntry> and would allow us to make arrays of IEntry for minor performance
        increases where applicable
         */
        public EntryType EntryType { get; set; }
    }
}
