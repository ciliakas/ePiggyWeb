using System;
using System.Collections.Generic;
using System.Linq;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataBase
{
    public static class EntryDbUpdater
    {
        public static int Add(Entry localEntry, int userId, EntryType entryType)
        {
            var db = new DatabaseContext();
            int id;
            if (entryType == EntryType.Income)
            {
                var entry = new IncomeModel(localEntry, userId);
                db.Add(entry);
                id = entry.Id;
            }
            else
            {
                var entry = new ExpenseModel(localEntry, userId);
                db.Add(entry);
                id = entry.Id;
            }
            db.SaveChanges();
            return id;
        }

        public static bool Remove(Entry entry, EntryType entryType)
        {
            var db = new DatabaseContext();
            try
            {
                if (entryType == EntryType.Income)
                {
                    var dbEntry = db.Incomes.FirstOrDefault(x => x.Id == entry.Id);
                    db.Incomes.Remove(dbEntry ?? throw new InvalidOperationException());
                }
                else
                {
                    var dbEntry = db.Expenses.FirstOrDefault(x => x.Id == entry.Id);
                    db.Expenses.Remove(dbEntry ?? throw new InvalidOperationException());
                }
                db.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                ExceptionHandler.Log("Couldn't find entry id: " + entry.Id + " in database");
                return false;
            }

            return true;
        }

        public static bool Edit(Entry oldEntry, Entry updatedEntry, EntryType entryType)
        {
            var db = new DatabaseContext();

            if (entryType == EntryType.Income)
            {
                var dbEntry = db.Incomes.FirstOrDefault(x => x.Id == oldEntry.Id);
                if (dbEntry == null)
                {
                    ExceptionHandler.Log("Couldn't find entry id: " + oldEntry.Id + " in database");
                    return false;
                }
                dbEntry.Edit(updatedEntry);
            }
            else
            {
                var dbEntry = db.Expenses.FirstOrDefault(x => x.Id == oldEntry.Id);
                if (dbEntry == null)
                {
                    ExceptionHandler.Log("Couldn't find entry id: " + oldEntry.Id + " in database");
                    return false;
                }
                dbEntry.Edit(updatedEntry);
            }
            db.SaveChanges();
            return true;
        }

        public static bool RemoveRange(EntryList entryList)
        {
            var db = new DatabaseContext();

            if (entryList.EntryType == EntryType.Income)
            {
                var entriesToRemove = entryList.Select(entry => db.Incomes.FirstOrDefault(x => x.Id == entry.Id)).ToList();
                db.Incomes.RemoveRange(entriesToRemove);
            }
            else
            {
                var entriesToRemove = entryList.Select(entry => db.Expenses.FirstOrDefault(x => x.Id == entry.Id)).ToList();
                db.Expenses.RemoveRange(entriesToRemove);
            }
            db.SaveChanges();
            return true;
        }

        //As I understand, all the entry lists that were passed to this Method will now have amended Id's and user Id's and are ready to be added locally 
        public static bool AddRange(EntryList entryList, int userId)
        {
            var db = new DatabaseContext();

            if (entryList.EntryType == EntryType.Income)
            {
                var dbEntryList = new List<IncomeModel>();
                foreach (var entry in entryList)
                {
                    var dbEntry = new IncomeModel(entry, userId);
                    dbEntryList.Add(dbEntry);
                    entry.Id = dbEntry.Id;
                    entry.UserId = userId;
                }
                db.AddRange(dbEntryList);
            }
            else
            {
                var dbEntryList = new List<ExpenseModel>();
                foreach (var entry in entryList)
                {
                    var dbEntry = new ExpenseModel(entry, userId);
                    dbEntryList.Add(dbEntry);
                    entry.Id = dbEntry.Id;
                }
                db.AddRange(dbEntryList);
            }

            db.SaveChanges();
            return true;
        }
    }
}
