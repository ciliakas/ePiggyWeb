using System;
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
        public int UserId { get; } = 0;

        public EntryManager(IEntryList entryList, int userId = 0)
        {
            EntryList = entryList;
            UserId = userId;
            ReadFromDb();
        }

        public bool Add(IEntry entry)
        {
            //Should check if valid id or something
            var id = EntryDbUpdater.Add(entry, UserId, EntryList.EntryType);

            if (id <= 0)
            {
                ExceptionHandler.Log("Invalid id of entry");
                return false;
            }
            EntryList.Add(new Entry(id, UserId, entry));
            return true;
        }

        public bool AddRange(IEntryEnumerable entryList)
        {
            if (!EntryDbUpdater.AddRange(entryList, UserId))
            {
                return false;
            }
            EntryList.AddRange(entryList);
            return true;
        }

        public bool Edit(IEntry oldEntry, IEntry newEntry)
        {
            //If something went wrong with database update return false
            if (!EntryDbUpdater.Edit(oldEntry, newEntry, EntryList.EntryType))
            {
                return false;
            }
            var temp = EntryList.FirstOrDefault(x => x.Id == oldEntry.Id);
            //If couldn't find the entry return false
            if (temp is null)
            {
                ExceptionHandler.Log("Edited entry id: " + oldEntry.Id + " in database but couldn't find it locally");
                return false;
            }
            temp.Edit(newEntry);
            return true;
        }

        public bool Remove(IEntry entry)
        {
            if (!EntryDbUpdater.Remove(entry, EntryList.EntryType)) return false;
            var temp = EntryList.FirstOrDefault(x => x.Id == entry.Id);
            if (temp is null)
            {
                ExceptionHandler.Log("Removed entry id: " + entry.Id + " from database but couldn't find it locally");
                return false;
            }
            EntryList.Remove(temp);
            return true;
        }

        public bool RemoveAll(IEntryList entryList)
        {
            if (!EntryDbUpdater.RemoveAll(entryList))
            {
                return false;
            }
            var temp = new EntryList(EntryType.Income);
            EntryList.RemoveAll(entryList.Contains);
            return true;
        }

        public bool ReadFromDb()
        {
            using var db = new DatabaseContext();
            
            switch (EntryList.EntryType)
            {
                case EntryType.Income:
                    foreach (var dbEntry in db.Incomes.Where(x => x.UserId == UserId)) // query executed and data obtained from database
                    {
                        var newEntry = new Entry(dbEntry);
                        EntryList.Add(newEntry);
                    }
                    break;
                case EntryType.Expense:
                    foreach (var dbEntry in db.Expenses.Where(x => x.UserId == UserId)) // query executed and data obtained from database
                    {
                        var newEntry = new Entry(dbEntry);
                        EntryList.Add(newEntry);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        public override string ToString()
        {
            return EntryList.ToString();
        }
    }
}
