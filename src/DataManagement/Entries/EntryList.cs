using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
    public class EntryList : List<IEntry>, IEntryList
    {
        //EntryType is to save up on the amount of times we have to pass the EntryType in methods
        public EntryType EntryType { get; set; }
        public EntryList(EntryType entryType)
        {
            EntryType = entryType;
        }

        private EntryList(EntryType entryType, IEnumerable<IEntry> entryList)
        {
            EntryType = entryType;
            if (entryList is null) return;
            AddRange(entryList);
        }
        
        public IEntryList GetBy(Importance importance)
        {
            return new EntryList(EntryType, this.Where(x => x.Importance == (int)importance).ToList());
        }

        public IEntryList GetBy(DateTime dateTime)
        {
            return new EntryList(EntryType, this.Where(x => x.Date.Year == dateTime.Year && x.Date.Month == dateTime.Month).ToList());
        }

        public IEntryList GetBy(DateTime from, DateTime to)
        {
            return new EntryList(EntryType, this.Where(x => x.Date >= from && x.Date <= to).ToList());
        }

        public IEntryList GetBy(bool recurring)
        {
            return new EntryList(EntryType, this.Where(x => x.Recurring == recurring).ToList());
        }

        public IEntryList GetUntilToday()
        {
            return new EntryList(EntryType, this.Where(x => x.Date <= DateTime.Today).ToList());
        }

        public DateTime GetOldestEntryDate()
        {
            return this.Min(x => x.Date);
        }

        public DateTime GetNewestEntryDate()
        {
            return this.Max(x => x.Date);
        }

        public decimal GetSum()
        {
            return this.Sum(entry => entry.Amount);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var entry in this)
            {
                sb.Append(entry);
                //sb.Append("\n");
            }

            return sb.ToString();
        }
    }
}
