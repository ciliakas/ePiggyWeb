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
        /*
        I've made all methods here require an EntryType instead of having to call different Add Income or Add Expense methods everywhere else
        Also moved to a standard of just passing IEntry everywhere, instead of many types of different parameters, because that quickly clogged up the code in this class.
         */
        public static int Add(IEntry localEntry, int userId, EntryType entryType)
        {
            var db = new DatabaseContext();
            if (entryType == EntryType.Income)
            {
                var entry = new IncomeModel(localEntry, userId);
                db.Add(entry);
                db.SaveChanges();
                return entry.Id;
            }
            else
            {
                var entry = new ExpenseModel(localEntry, userId);
                db.Add(entry);
                db.SaveChanges();
                return entry.Id;
            }
        }

        public static bool Remove(int id, int userId, EntryType entryType)
        {
            var db = new DatabaseContext();
            try
            {
                if (entryType == EntryType.Income)
                {
                    var dbEntry = db.Incomes.FirstOrDefault(x => x.Id == id && x.UserId == userId);
                    db.Incomes.Remove(dbEntry ?? throw new InvalidOperationException());
                }
                else
                {
                    var dbEntry = db.Expenses.FirstOrDefault(x => x.Id == id && x.UserId == userId);
                    db.Expenses.Remove(dbEntry ?? throw new InvalidOperationException());
                }
                db.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                ExceptionHandler.Log("Couldn't find entry id: " + id + " in database");
                return false;
            }

            return true;
        }

        public static bool Edit(int id, IEntry updatedEntry, EntryType entryType)
        {
            var db = new DatabaseContext();

            if (entryType == EntryType.Income)
            {
                var dbEntry = db.Incomes.FirstOrDefault(x => x.Id == id && x.UserId == updatedEntry.UserId);
                if (dbEntry == null)
                {
                    ExceptionHandler.Log("Couldn't find entry id: " + id + " in database");
                    return false;
                }
                dbEntry.Edit(updatedEntry);
            }
            else
            {
                var dbEntry = db.Expenses.FirstOrDefault(x => x.Id == id && x.UserId == updatedEntry.UserId);
                if (dbEntry == null)
                {
                    ExceptionHandler.Log("Couldn't find entry id: " + id + " in database");
                    return false;
                }
                dbEntry.Edit(updatedEntry);
            }
            db.SaveChanges();
            return true;
        }

        public static bool RemoveAll(IEnumerable<int> idArray, EntryType entryType)
        {
            var db = new DatabaseContext();

            if (entryType == EntryType.Income)
            {
                var entriesToRemove = idArray.Select(id => db.Incomes.FirstOrDefault(x => x.Id == id)).ToList();
                db.Incomes.RemoveRange(entriesToRemove);
            }
            else
            {
                var entriesToRemove = idArray.Select(id => db.Expenses.FirstOrDefault(x => x.Id == id)).ToList();
                db.Expenses.RemoveRange(entriesToRemove);
            }
            db.SaveChanges();
            return true;
        }

        //As I understand, all the entry lists that were passed to this Method will now have amended Id's and user Id's and are ready to be added locally 
        public static bool AddRange(IEntryEnumerable entryList, int userId)
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
