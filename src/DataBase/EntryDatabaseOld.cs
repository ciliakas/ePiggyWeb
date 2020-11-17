using System;
using System.Collections.Generic;
using System.Linq;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataBase
{
    public static class EntryDatabaseOld
    {
        /*
        I've made all methods here require an EntryType instead of having to call different Add Income or Add Expense methods everywhere else
        Also moved to a standard of just passing IEntry everywhere, instead of many types of different parameters, because that quickly clogged up the code in this class.
         */

        public static int Create(IEntry localEntry, int userId, EntryType entryType)
        {
            if (localEntry is null)
            {
                return -1;
            }

            if (!localEntry.Recurring)
            {
                return CreateSingle(localEntry, userId, entryType);
            }

            CreateList(RecurringUpdater.CreateRecurringList(localEntry, entryType), userId);
            return -2;
        }

        public static int CreateSingle(IEntry localEntry, int userId, EntryType entryType)
        {
            using var db = new DatabaseContext();
            IEntryModel dbEntry = entryType switch
            {
                EntryType.Income => new IncomeModel(localEntry, userId),
                _ => new ExpenseModel(localEntry, userId),
            };
            db.Add(dbEntry);
            db.SaveChanges();
            return dbEntry.Id;
        }

        public static bool CreateList(IEntryList entryList, int userId)
        {
            using var db = new DatabaseContext();
            var dictionary = new Dictionary<IEntry, IEntryModel>();
            foreach (var entry in entryList)
            {
                IEntryModel dbEntry = entryList.EntryType switch
                {
                    EntryType.Income => new IncomeModel(entry, userId),
                    _ => new ExpenseModel(entry, userId),
                };
                dictionary.Add(entry, dbEntry);
            }
            // Setting all of the ID's to local Entries, just so this method remains usable both with local and only database usage
            db.AddRange(dictionary.Values);
            db.SaveChanges();
            entryList.Clear();
            foreach (var (key, value) in dictionary)
            {
                key.Id = value.Id;
                entryList.Add(key);
            }

            return true;
        }

        public static bool Update(int id, int userId, IEntry updatedEntry, EntryType entryType)
        {
            if (updatedEntry is null)
            {
                return false;
            }

            if (updatedEntry.Recurring)
            {
                CreateList(RecurringUpdater.CreateRecurringListWithoutOriginalEntry(updatedEntry, entryType), userId);
                updatedEntry.Recurring = false;
            }

            return UpdateSingle(id, userId, updatedEntry, entryType);
        }

        public static bool UpdateSingle(int id, int userId, IEntry updatedEntry, EntryType entryType)
        {
            using var db = new DatabaseContext();
            IEntryModel temp = entryType switch
            {
                EntryType.Income => db.Incomes.FirstOrDefault(x => x.Id == id && x.UserId == userId),
                _ => db.Expenses.FirstOrDefault(x => x.Id == id && x.UserId == userId)
            };

            if (temp is null)
            {
                ExceptionHandler.Log("Couldn't find entry id: " + id + " in database");
                return false;
            }

            updatedEntry.UserId = userId;
            temp.Edit(updatedEntry);
            db.SaveChanges();
            return true;
        }

        public static bool Delete(IEntry entry, EntryType entryType)
        {
            return Delete(x => x.Id == entry.Id && x.UserId == entry.UserId, entryType);
        }

        public static bool Delete(int id, int userId, EntryType entryType)
        {
            return Delete(x => x.Id == id && x.UserId == userId, entryType);
        }

        public static bool Delete(Func<IEntryModel, bool> filter, EntryType entryType)
        {
            using var db = new DatabaseContext();
            try
            {
                var dbEntry = entryType switch
                {
                    EntryType.Income => db.Incomes.FirstOrDefault(filter),
                    _ => db.Expenses.FirstOrDefault(filter)
                };
                db.Remove(dbEntry ?? throw new InvalidOperationException());
                db.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                return false;
            }

            return true;
        }

        public static bool DeleteList(Func<IEntryModel, bool> filter, EntryType entryType)
        {
            var db = new DatabaseContext();

            if (entryType == EntryType.Income)
            {
                var entriesToRemove = db.Incomes.Where(filter).Cast<IncomeModel>().ToList();
                db.Incomes.RemoveRange(entriesToRemove);
            }
            else
            {
                var entriesToRemove = db.Expenses.Where(filter).Cast<ExpenseModel>().ToList();
                db.Expenses.RemoveRange(entriesToRemove);
            }

            db.SaveChanges();
            return true;
        }

        public static bool DeleteList(IEnumerable<int> idArray, int userId, EntryType entryType)
        {
            var db = new DatabaseContext();

            if (entryType == EntryType.Income)
            {
                var entriesToRemove = idArray.Select(id => db.Incomes.FirstOrDefault(x => x.Id == id && x.UserId == userId)).ToList();
                db.Incomes.RemoveRange(entriesToRemove);
            }
            else
            {
                var entriesToRemove = idArray.Select(id => db.Expenses.FirstOrDefault(x => x.Id == id && x.UserId == userId)).ToList();
                db.Expenses.RemoveRange(entriesToRemove);
            }

            db.SaveChanges();
            return true;
        }

        public static IEntry Read(int id, int userId, EntryType entryType)
        {
            using var db = new DatabaseContext();
            IEntryModel dbEntry = entryType switch
            {
                EntryType.Income => db.Incomes.FirstOrDefault(x => x.Id == id && x.UserId == userId),
                _ => db.Expenses.FirstOrDefault(x => x.Id == id && x.UserId == userId)
            };
            return new Entry(dbEntry);
        }

        public static IEnumerable<IEntry> ReadList(int userId, EntryType entryType)
        {
            return ReadList(x => x.UserId == userId, entryType);
        }

        public static IEnumerable<IEntry> ReadList(Func<IEntryModel, bool> filter, EntryType entryType)
        {
            using var db = new DatabaseContext();
            IEnumerable<IEntry> temp = entryType switch
            {
                EntryType.Income => db.Incomes.Where(filter).Select(dbEntry => new Entry(dbEntry)).Cast<IEntry>().ToList(),
                _ => db.Expenses.Where(filter).Select(dbEntry => new Entry(dbEntry)).Cast<IEntry>().ToList()
            };
            return temp;
        }
    }
}
