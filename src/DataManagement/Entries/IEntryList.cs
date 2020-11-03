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
        public IEntryList GetBy(Importance importance, DateTime from, DateTime to);
        public IEntryList GetBy(Importance importance, DateTime dateTime);
        public IEntryList GetBy(bool recurring);
        public IEntryList GetUntilToday();
        public IEntry GetOldestEntry();
        public IEntry GetNewestEntry();
        public decimal GetSum();
        public decimal GetSum(Importance importance);
        public decimal GetSum(DateTime dateTime);
        public decimal GetSum(DateTime from, DateTime to);
        public decimal GetSum(Importance importance, DateTime from, DateTime to);
        public decimal GetSum(Importance importance, DateTime dateTime);
        public decimal GetSum(bool recurring);
        public decimal GetSumUntilToday();

        //public IEntryList GetUntilEndOfCurrentMonth(); // bet cia realiai tas pats kas duoti tiesiog sita menesi tai...
    }
}
