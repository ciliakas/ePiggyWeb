using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntryList : IList<IEntry>, IEntryEnumerable
    {
        public IEntryList GetBy(Importance importance);
        public IEntryList GetBy(DateTime dateTime);
        public IEntryList GetBy(DateTime from, DateTime to);
        public IEntryList GetBy(bool recurring);
        public IEntryList GetUntilToday();
        public DateTime GetOldestEntry();
        public DateTime GetNewestEntry();
        public decimal GetSum();

        //public IEntryList GetUntilEndOfCurrentMonth(); // bet cia realiai tas pats kas duoti tiesiog sita menesi tai...
    }
}
