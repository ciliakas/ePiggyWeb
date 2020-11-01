using System;
using System.Linq;
using ePiggyWeb.DataBase;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
    public class EntryManager
    {
        public EntryList EntryList { get; }

        //Somehow I should get user id here
        private int UserId { get; } = 0;

        public EntryManager(EntryType entryType)
        {
            EntryList = new EntryList(entryType);
            ReadFromDb();
        }

        public bool Add(Entry entry)
        {
            //Should check if valid id or something
            var id = EntryDbUpdater.Add(entry, UserId, EntryList.EntryType);

            if (id <= 0)
            {
                ExceptionHandler.Log("Invalid id of entry");
                return false;
            }
            EntryList.Add(new Entry(entry, id, UserId));
            return true;
        }

        public bool AddRange(EntryList entryList)
        {
            if (!EntryDbUpdater.AddRange(entryList, UserId))
            {
                return false;
            }
            EntryList.AddRange(entryList);
            return true;
        }

        public bool Edit(Entry oldEntry, Entry newEntry)
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

        public bool Remove(Entry entry)
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

        public bool RemoveRange(EntryList entryList)
        {
            if (!EntryDbUpdater.RemoveRange(entryList))
            {
                return false;
            }
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
