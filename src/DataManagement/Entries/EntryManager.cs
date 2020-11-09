using System;
using System.Collections.Generic;
using System.Linq;
using ePiggyWeb.DataBase;
using ePiggyWeb.Utilities;
using IListExtension;

namespace ePiggyWeb.DataManagement.Entries
{
    public class EntryManager : IEntryManager
    {
        /*
        So EntryManager (and by extension GoalManager) basically takeover the same functionality as old Data.cs class used to do.
        EntryManager reflects all the change we do on a local list to the database as well.
        In other words, if we want to add a entry, we call the EntryManager.Add method, which will first update the database, 
        and if the database was successfully updated, it will update the local list as well, and analogously for every other method in EntryManager.
        Also EntryManager has the duty of reading the data from the database on it's creation.
       */
        public IEntryList EntryList { get; }

        //Somehow I should get user id here
        public int UserId { get; }

        public EntryManager(IEntryList entryList, int userId = 0)
        {
            EntryList = entryList;
            UserId = userId;
            ReadFromDb();
        }

        public bool Add(IEntry entry)
        {
            if (entry is null)
            {
                return false;
            }
            if (entry.Recurring)
            {
                return AddRange(RecurringUpdater.CreateRecurringList(entry, EntryList.EntryType));
            }

            var id = EntryDatabase.CreateSingle(entry, UserId, EntryList.EntryType);
            if (id <= 0)
            {
                ExceptionHandler.Log("Invalid id of entry");
                return false;
            }
            EntryList.Add(new Entry(id, UserId, entry));
            return true;
        }

        public bool AddRange(IEntryList entryList)
        {
            if (!EntryDatabase.CreateList(entryList, UserId))
            {
                return false;
            }
            EntryList.AddRange(entryList);
            return true;
        }

        public bool Edit(IEntry oldEntry, IEntry updatedEntry)
        {
            return Edit(oldEntry.Id, updatedEntry);
        }

        public bool Edit(int id, IEntry updatedEntry)
        {
            if (updatedEntry is null)
            {
                return false;
            }

            if (updatedEntry.Recurring)
            {
                var list = RecurringUpdater.CreateRecurringListWithoutOriginalEntry(updatedEntry, EntryList.EntryType);
                EntryDatabase.CreateList(list, UserId);
                updatedEntry.Recurring = false;
            }

            //If something went wrong with database update return false
            if (!EntryDatabase.UpdateSingle(id, UserId, updatedEntry, EntryList.EntryType))
            {
                return false;
            }
            var temp = EntryList.FirstOrDefault(x => x.Id == id);
            //If couldn't find the entry return false
            if (temp is null)
            {
                ExceptionHandler.Log("Edited entry id: " + id + " in database but couldn't find it locally");
                return false;
            }
            temp.Edit(updatedEntry);
            return true;
        }

        public bool Remove(IEntry entry)
        {
            return Remove(entry.Id);
        }

        public bool Remove(int id)
        {
            if (!EntryDatabase.Delete(id, EntryList.EntryType))
            {
                return false;
            }
            var temp = EntryList.FirstOrDefault(x => x.Id == id);
            if (temp is null)
            {
                ExceptionHandler.Log("Removed entry id: " + id + " from database but couldn't find it locally");
                return false;
            }
            EntryList.Remove(temp);
            return true;
        }

        public bool RemoveAll(IEntryList entryList)
        {
            var idList = entryList.Select(va => va.Id).ToArray();
            if (!EntryDatabase.DeleteList(idList, entryList.EntryType))
            {
                return false;
            }
            var temp = new EntryList(EntryType.Income);
            EntryList.RemoveAll(entryList.Contains);
            return true;
        }

        public bool RemoveAll(IEnumerable<int> idList)
        {
            var idArray = idList as int[] ?? idList.ToArray();
            if (!EntryDatabase.DeleteList(idArray, EntryList.EntryType))
            {
                return false;
            }

            var temp = new EntryList(EntryList.EntryType);
            temp.AddRange(from entry in EntryList from id in idArray where entry.Id == id select entry);

            foreach (var entry in temp)
            {
                EntryList.Remove(entry);
            }

            return true;
        }

        public bool ReadFromDb()
        {
            using var db = new DatabaseContext();
            EntryList.AddRange(EntryDatabase.ReadList(UserId, EntryList.EntryType));
            return true;
        }

        public override string ToString()
        {
            return EntryList.ToString();
        }
    }
}
