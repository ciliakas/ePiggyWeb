using System.Collections.Generic;
using ePiggyWeb.DataManagement.Entries;

namespace ePiggyWeb.Utilities
{
    public static class ArrayExtension
    {
        public static IEntryList ToIEntryList(this IEnumerable<IEntry> entryEnumerable)
        {
            return new EntryList(entryEnumerable);
        }
    }
}
