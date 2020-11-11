using System.Collections.Generic;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntryManager
    {
        public IEntryList EntryList { get; }
        public int UserId { get; }
        public bool Add(IEntry entry);
        public bool AddRange(IEntryList entryList);
        public bool Edit(IEntry oldEntry, IEntry updatedEntry);
        public bool Edit(int id, IEntry updatedEntry);
        public bool Remove(IEntry entry);
        public bool Remove(int id);
        public bool RemoveAll(IEntryList entryList);
        public bool RemoveAll(IEnumerable<int> idList);
    }
}
