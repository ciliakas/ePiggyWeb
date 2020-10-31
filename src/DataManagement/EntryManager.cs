using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ePiggyWeb.DataManagement
{
    public class EntryManager
    {
        public EntryList EntryList { get; }

        public EntryManager(EntryList entryList)
        {
            EntryList = entryList;
            var usedId = 0;
            ReadFromDb(usedId);
        }

        public bool Add(Entry entry)
        {
            // get user id
            var userId = 0;

            //Should check if valid id or something
            var id = EntryDbUpdater.AddEntry(entry, userId, EntryList.EntryType);

            if (id <= 0)
            {
                ExceptionHandler.Log("Invalid id of entry");
                return false;
            }
            EntryList.Add(new Entry(entry, id, userId));
            return true;
        }

        public bool AddRange(EntryList entryList)
        {
            var userId = 0;
            if (!EntryDbUpdater.AddEntryList(entryList, userId))
            {
                return false;
            }
            EntryList.AddRange(entryList);
            return true;
        }

        public bool Edit(Entry oldEntry, Entry newEntry)
        {
            //If something went wrong with database update return false
            if (!EntryDbUpdater.EditEntry(oldEntry, newEntry, EntryList.EntryType))
            {
                return false;
            }
            var temp = EntryList.FirstOrDefault(x => x.Id == oldEntry.Id);
            //If couldn't find the entry return false
            if (temp is null)
            {
                return false;
            }
            temp.Edit(newEntry);
            return true;
        }

        public bool Remove(Entry entry)
        {
            if (!EntryDbUpdater.RemoveEntry(entry, EntryList.EntryType)) return false;
            var temp = EntryList.FirstOrDefault(x => x.Id == entry.Id);
            if (temp is null)
            {
                return false;
            }
            EntryList.Remove(temp);
            return true;
        }

        public bool RemoveList(EntryList entryList)
        {
            if (!EntryDbUpdater.RemoveEntryList(entryList))
            {
                return false;
            }
            EntryList.RemoveAll(entryList.Contains);
            return true;

        }

        public bool ReadFromDb(int userId)
        {
            using var context = new DatabaseContext();
            switch (EntryList.EntryType)
            {
                case EntryType.Income:
                    foreach (var entry in context.Incomes.Where(x => x.UserId == userId)) // query executed and data obtained from database
                    {
                        var newEntry = new Entry(entry.Id, entry.UserId, entry.Amount, entry.Title, entry.Date, entry.IsMonthly, entry.Importance);
                        EntryList.Add(newEntry);
                    }
                    break;
                case EntryType.Expense:
                    foreach (var entry in context.Expenses.Where(x => x.UserId == userId)) // query executed and data obtained from database
                    {
                        var newEntry = new Entry(entry.Id, entry.UserId, entry.Amount, entry.Title, entry.Date, entry.IsMonthly, entry.Importance);
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
