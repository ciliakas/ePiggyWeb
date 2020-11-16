using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ePiggyWeb.DataBase
{
    public class EntryDatabase
    {
        private PiggyDbContext Database { get; }
        public EntryDatabase(PiggyDbContext database)
        {
            Database = database;
        }

        public async Task<int> CreateAsync(IEntry localEntry, int userId, EntryType entryType)
        {
            if (localEntry is null)
            {
                return -1;
            }

            if (!localEntry.Recurring)
            {
                return await CreateSingleAsync(localEntry, userId, entryType);
            }

            await CreateListAsync(RecurringUpdater.CreateRecurringList(localEntry, entryType), userId);
            return -2;
        }

        public async Task<int> CreateSingleAsync(IEntry localEntry, int userId, EntryType entryType)
        {
            IEntryModel dbEntry = entryType switch
            {
                EntryType.Income => new IncomeModel(localEntry, userId),
                _ => new ExpenseModel(localEntry, userId),
            };
            await Database.AddAsync(dbEntry);
            await Database.SaveChangesAsync();
            return dbEntry.Id;
        }

        public async Task<bool> CreateListAsync(IEntryList entryList, int userId)
        {
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
            await Database.AddRangeAsync(dictionary.Values);
            await Database.SaveChangesAsync();
            entryList.Clear();
            foreach (var (key, value) in dictionary)
            {
                key.Id = value.Id;
                entryList.Add(key);
            }

            return true;
        }


        public async Task<bool> UpdateAsync(int id, int userId, IEntry updatedEntry, EntryType entryType)
        {
            if (updatedEntry is null)
            {
                return false;
            }

            if (updatedEntry.Recurring)
            {
                await CreateListAsync(RecurringUpdater.CreateRecurringListWithoutOriginalEntry(updatedEntry, entryType), userId);
                updatedEntry.Recurring = false;
            }

            return await UpdateSingleAsync(id, userId, updatedEntry, entryType);
        }

        public async Task<bool> UpdateSingleAsync(int id, int userId, IEntry updatedEntry, EntryType entryType)
        {
            IEntryModel temp = entryType switch
            {
                EntryType.Income => await Database.Incomes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId),
                _ => await Database.Expenses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId)
            };

            if (temp is null)
            {
                ExceptionHandler.Log("Couldn't find entry id: " + id + " in database");
                return false;
            }

            updatedEntry.UserId = userId;
            temp.Edit(updatedEntry);
            await Database.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> DeleteAsync(IEntry entry, EntryType entryType)
        {
            return await DeleteAsync(x => x.Id == entry.Id && x.UserId == entry.UserId, entryType);
        }

        public async Task<bool> DeleteAsync(int id, int userId, EntryType entryType)
        {
            return await DeleteAsync(x => x.Id == id && x.UserId == userId, entryType);
        }

        public async Task<bool> DeleteAsync(Expression<Func<IEntryModel, bool>> filter, EntryType entryType)
        {
            try
            {
                var dbEntry = entryType switch
                {
                    EntryType.Income => Database.Incomes.FirstOrDefault(filter),
                    _ => Database.Expenses.FirstOrDefault(filter)
                };
                Database.Remove(dbEntry ?? throw new InvalidOperationException());
                await Database.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteListAsync(Expression<Func<IEntryModel, bool>> filter, EntryType entryType)
        {
            IEnumerable<IEntryModel> entriesToRemove = entryType switch
            {
                EntryType.Income => await Database.Incomes.Where(filter).ToListAsync(),
                _ => await Database.Expenses.Where(filter).ToListAsync()
            };
            Database.RemoveRange(entriesToRemove);
            await Database.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteListAsync(IEnumerable<int> idArray, int userId, EntryType entryType)
        {
            IEnumerable<IEntryModel> entriesToRemove = entryType switch
            {
                EntryType.Income => await Task.WhenAll(idArray.Select(selector: async id =>
                    await Database.Incomes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId))),
                _ => await Task.WhenAll(idArray.Select(selector: async id =>
                    await Database.Expenses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId)))
            };
            Database.RemoveRange(entriesToRemove);
            await Database.SaveChangesAsync();
            return true;
        }

        public async Task<IEntry> ReadAsync(int id, int userId, EntryType entryType)
        {
            IEntryModel dbEntry = entryType switch
            {
                EntryType.Income => Database.Incomes.FirstOrDefault(x => x.Id == id && x.UserId == userId),
                _ => Database.Expenses.FirstOrDefault(x => x.Id == id && x.UserId == userId)
            };
            return await ReadAsync(x => x.Id == id && x.UserId == userId, entryType);
        }

        public async Task<IEntry> ReadAsync(Expression<Func<IEntryModel, bool>> filter, EntryType entryType)
        {
            var dbEntry = entryType switch
            {
                EntryType.Income => await Database.Incomes.FirstOrDefaultAsync(filter),
                _ => await Database.Expenses.FirstOrDefaultAsync(filter)
            };
            return new Entry(dbEntry);
        }

        public async Task<IEntryList> ReadListAsync(int userId, EntryType entryType)
        {
            return await ReadListAsync(x => x.UserId == userId, entryType);
        }

        public async Task<IEntryList> ReadListAsync(Expression<Func<IEntryModel, bool>> filter, EntryType entryType)
        {
            var list = new EntryList(entryType);
            IEnumerable<IEntry> temp = entryType switch
            {
                EntryType.Income => await Database.Incomes.Where(filter).Select(dbEntry => new Entry(dbEntry)).Cast<IEntry>().ToListAsync(),
                _ => await Database.Expenses.Where(filter).Select(dbEntry => new Entry(dbEntry)).Cast<IEntry>().ToListAsync()
            };
            list.AddRange(temp);
            return list;
        }
    }
}
