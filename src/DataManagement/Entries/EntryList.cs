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
            throw new NotImplementedException();
        }

        public IEntryList GetBy(DateTime @from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public IEntryList GetBy(Importance importance, DateTime @from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public IEntryList GetBy(Importance importance, DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public IEntryList GetBy(bool recurring)
        {
            throw new NotImplementedException();
        }

        public IEntryList GetUntilToday()
        {
            throw new NotImplementedException();
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
