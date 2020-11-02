using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataBase
{
    public static class EntryDbUpdater
    {
        public static int AddEntry(Entry localEntry, int userId, EntryType entryType)
        {
            var db = new DatabaseContext();
            if (entryType == EntryType.Income)
            {
                var entry = new Incomes(localEntry, userId);
                db.Add(entry);
                db.SaveChanges();
                return entry.Id;
            }
            else
            {
                var entry = new Expenses(localEntry, userId);
                db.Add(entry);
                db.SaveChanges();
                return entry.Id;
            }
        }

        public static bool RemoveEntry(Entry entry, EntryType entryType)
        {
            var db = new DatabaseContext();
            try
            {
                if (entryType == EntryType.Income)
                {
                    var index = db.Incomes.FirstOrDefault(x => x.Id == entry.Id);
                    db.Incomes.Remove(index ?? throw new InvalidOperationException());
                }
                else
                {
                    var index = db.Expenses.FirstOrDefault(x => x.Id == entry.Id);
                    db.Expenses.Remove(index ?? throw new InvalidOperationException());
                }
                db.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                return false;
            }

            return true;
        }

        public static bool EditEntry(Entry oldEntry, Entry updatedEntry, EntryType entryType)
        {
            var db = new DatabaseContext();

            if (entryType == EntryType.Income)
            {
                var temp = db.Incomes.FirstOrDefault(x => x.Id == oldEntry.Id);
                if (temp == null)
                {
                    ExceptionHandler.Log("Couldn't find entry");
                    return false;
                }
                temp.Edit(updatedEntry);
            }
            else
            {

                var temp = db.Expenses.FirstOrDefault(x => x.Id == oldEntry.Id);
                if (temp == null)
                {
                    ExceptionHandler.Log("Couldn't find entry");
                    return false;
                }
                temp.Edit(updatedEntry);
            }
            db.SaveChanges();
            return true;
        }

        public static bool RemoveEntryList(EntryList entryList)
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
        public static bool AddEntryList(EntryList entryList, int userId)
        {
            var db = new DatabaseContext();

            if (entryList.EntryType == EntryType.Income)
            {
                var databaseEntryList = new List<Incomes>();
                foreach (var entry in entryList)
                {
                    var databaseEntry = new Incomes(entry, userId);
                    databaseEntryList.Add(databaseEntry);
                    entry.Id = databaseEntry.Id;
                    entry.UserId = userId;
                }
                db.AddRange(databaseEntryList);
            }
            else
            {
                var databaseEntryList = new List<Expenses>();
                foreach (var entry in entryList)
                {
                    var databaseEntry = new Expenses(entry, userId);
                    databaseEntryList.Add(databaseEntry);
                    entry.Id = databaseEntry.Id;
                }
                db.AddRange(databaseEntryList);
            }

            db.SaveChanges();
            return true;
        }
    }
}
