using System;
using System.Collections.Generic;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntryList : IList<IEntry>, IEntryEnumerable
    {
        public IEntryList GetBy(Importance importance);
        public IEntryList GetBy(DateTime dateTime);
        public IEntryList GetFrom(DateTime dateTime);
        public IEntryList GetTo(DateTime dateTime);
        public IEntryList GetBy(bool recurring);
        public IEntryList GetUntilToday();
        public IEntryList GetPage(int pageNumber, int pageSize);
        public DateTime GetOldestEntryDate();
        public DateTime GetNewestEntryDate();
        public decimal GetSum();
    }
}
