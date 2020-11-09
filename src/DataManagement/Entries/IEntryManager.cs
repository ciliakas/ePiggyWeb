using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntryManager
    {
        public IEntryList EntryList { get; }
        public int UserId { get; }
        public bool Add(IEntry entry);
        public bool AddRange(IEntryList entryList);
        public bool Edit(IEntry oldEntry, IEntry updatedEntry);
        public bool Remove(IEntry entry);
        public bool RemoveAll(IEntryList entryList);
    }
}
