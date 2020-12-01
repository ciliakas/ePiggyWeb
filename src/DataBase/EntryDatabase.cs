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

            IEntryModel temp = entryType switch
            {
                EntryType.Income => await Database.Incomes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId),
                _ => await Database.Expenses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId)
            };

            if (temp is null)
            {
                throw new Exception("Couldn't find entry id: " + id + " in database");
            }

            if (updatedEntry.Recurring && !TimeManager.IsDateInFuture(updatedEntry.Date))
            {
                await CreateListAsync(RecurringUpdater.CreateRecurringListWithoutOriginalEntry(updatedEntry, entryType), userId);
                updatedEntry.Recurring = false;
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
            var dbEntry = entryType switch
            {
                EntryType.Income => await Database.Incomes.FirstOrDefaultAsync(filter),
                _ => await Database.Expenses.FirstOrDefaultAsync(filter)
            };
            Database.Remove(dbEntry ?? throw new InvalidOperationException());
            await Database.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteListAsync(Expression<Func<IEntryModel, bool>> filter, EntryType entryType)
        {
            IEnumerable<IEntryModel> entriesToRemove = entryType switch
            {
                EntryType.Income => await Database.Incomes.Where(filter).ToListAsync(),
                _ => await Database.Expenses.Where(filter).ToListAsync()
            };
            //var predicate = Expression<Func<IEntryModel, bool>>.Create();
            Database.RemoveRange(entriesToRemove);
            await Database.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteListAsync(IEnumerable<IEntry> entryArray, int userId, EntryType entryType)
        {
            var idArray = entryArray.Select(entry => entry.Id).ToArray();
            return await DeleteListAsync(idArray, userId, entryType);
        }

        public async Task<bool> DeleteListAsync(IEnumerable<int> idArray, int userId, EntryType entryType)
        {
            var filter = PredicateBuilder.BuildEntryFilter(idArray, userId);
            return await DeleteListAsync(filter, entryType);
        }

        public async Task<IEntry> ReadAsync(int id, int userId, EntryType entryType)
        {
            IEntryModel dbEntry = entryType switch
            {
                EntryType.Income => await Database.Incomes.FirstOrDefaultAsync(
                    x => x.Id == id && x.UserId == userId),
                _ => await Database.Expenses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId)
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
                EntryType.Income => await Database.Incomes.Where(filter).Select(dbEntry => new Entry(dbEntry) as IEntry).ToListAsync(),
                _ => await Database.Expenses.Where(filter).Select(dbEntry => new Entry(dbEntry) as IEntry).ToListAsync()
            };
            list.AddRange(temp);
            return list;
        }

        public async Task UpdateRecurringListAsync(Expression<Func<IEntryModel, bool>> filter, EntryType entryType)
        {
            var list = new EntryList(entryType);
            IEnumerable<IEntry> temp = entryType switch
            {
                EntryType.Income => await Database.Incomes.Where(filter).Select(dbEntry => new Entry(dbEntry) as IEntry).ToListAsync(),
                _ => await Database.Expenses.Where(filter).Select(dbEntry => new Entry(dbEntry) as IEntry).ToListAsync()
            };
            list.AddRange(temp);
        }
    }
}
