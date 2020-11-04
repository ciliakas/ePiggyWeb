using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntryList : IList<IEntry>, IEntryEnumerable
    {
        /*
        This interface is empty because it inherits from IEntryEnumerable, which has the Property we need already (EntryType)
        And it also calls for the class that implements this interface to implement IList<IEntry>
        So basically a List<IEntry> with a property of EntryType would successfully implement this interface
         */
        public IEntryList GetBy(Importance importance);
        public IEntryList GetBy(DateTime dateTime);
        public IEntryList GetBy(DateTime from, DateTime to);
        public IEntryList GetBy(bool recurring);
        public IEntryList GetUntilToday();
        public DateTime GetOldestEntryDate();
        public DateTime GetNewestEntryDate();
        public decimal GetSum();

    }
}
