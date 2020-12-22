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

        public async Task CreateAsync(IEntry localEntry, int userId, EntryType entryType)
        {
            if (localEntry is null)
            {
                throw new ArgumentException();
            }

            if (!localEntry.Recurring)
            {
                await CreateSingleAsync(localEntry, userId, entryType);
                return;
            }

            await CreateListAsync(RecurringUpdater.CreateRecurringList(localEntry, entryType), userId);
        }

        private async Task CreateSingleAsync(IEntry localEntry, int userId, EntryType entryType)
        {
            IEntryModel dbEntry = entryType switch
            {
                EntryType.Income => new IncomeModel(localEntry, userId),
                _ => new ExpenseModel(localEntry, userId),
            };

            await Database.AddAsync(dbEntry);
            await Database.SaveChangesAsync();
        }

        private async Task CreateListAsync(IEntryList entryList, int userId)
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
            await Database.AddRangeAsync(dictionary.Values);
            await Database.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, int userId, IEntry updatedEntry, EntryType entryType)
        {
            if (updatedEntry is null)
            {
                throw new ArgumentException();
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

            if (updatedEntry.Recurring && !TimeManager.IsDateThisMonthAndLater(updatedEntry.Date))
            {
                await CreateListAsync(RecurringUpdater.CreateRecurringListWithoutOriginalEntry(updatedEntry, entryType), userId);
                updatedEntry.Recurring = false;
            }

            updatedEntry.UserId = userId;
            temp.Edit(updatedEntry);
            await Database.SaveChangesAsync();
        }

        public async Task DeleteAsync(IEntry entry, EntryType entryType)
        {
            await DeleteAsync(x => x.Id == entry.Id && x.UserId == entry.UserId, entryType);
        }

        public async Task DeleteAsync(int id, int userId, EntryType entryType)
        {
            await DeleteAsync(x => x.Id == id && x.UserId == userId, entryType);
        }

        private async Task DeleteAsync(Expression<Func<IEntryModel, bool>> filter, EntryType entryType)
        {
            var dbEntry = entryType switch
            {
                EntryType.Income => await Database.Incomes.FirstOrDefaultAsync(filter),
                _ => await Database.Expenses.FirstOrDefaultAsync(filter)
            };
            Database.Remove(dbEntry ?? throw new InvalidOperationException());
            await Database.SaveChangesAsync();
        }

        private async Task DeleteListAsync(Expression<Func<IEntryModel, bool>> filter, EntryType entryType)
        {
            IEnumerable<IEntryModel> entriesToRemove = entryType switch
            {
                EntryType.Income => await Database.Incomes.Where(filter).ToListAsync(),
                _ => await Database.Expenses.Where(filter).ToListAsync()
            };
            //var predicate = Expression<Func<IEntryModel, bool>>.Create();
            Database.RemoveRange(entriesToRemove);
            await Database.SaveChangesAsync();
        }

        public async Task DeleteListAsync(IEnumerable<IEntry> entryArray, int userId, EntryType entryType)
        {
            var idArray = entryArray.Select(entry => entry.Id).ToArray();
            await DeleteListAsync(idArray, userId, entryType);
        }

        public async Task DeleteListAsync(IEnumerable<int> idArray, int userId, EntryType entryType)
        {
            var filter = PredicateBuilder.BuildEntryFilter(idArray, userId);
            await DeleteListAsync(filter, entryType);
        }

        public async Task<IEntry> ReadAsync(int id, int userId, EntryType entryType)
        {
            return await ReadAsync(x => x.Id == id && x.UserId == userId, entryType);
        }

        private async Task<IEntry> ReadAsync(Expression<Func<IEntryModel, bool>> filter, EntryType entryType)
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
            return await ReadListAsync(x => true, userId, entryType);
        }

        public async Task<Tuple<IEntryList, int>> ReadByPage(Expression<Func<IEntryModel, bool>> filter, int userId, EntryType entryType,
            int pageNumber, int pageSize)
        {
            await UpdateRecurringAsync(userId, entryType);
            var updatedFilter = filter.And(x => x.UserId == userId);

            var allEntries = entryType switch
            {
                EntryType.Income => await Database.Incomes.Where(updatedFilter).OrderByDescending(x => x.Date)
                    .Select(dbEntry => new Entry(dbEntry) as IEntry).ToListAsync(),
                _ => await Database.Expenses.Where(updatedFilter).OrderByDescending(x => x.Date)
                    .Select(dbEntry => new Entry(dbEntry) as IEntry).ToListAsync()
            };

            var list = new EntryList(entryType);
            list.AddRange(allEntries.Skip((pageNumber - 1) * pageSize).Take(pageSize));
            var numberOfPages = (int) Math.Ceiling(decimal.Divide(allEntries.Count, pageSize));
            return new Tuple<IEntryList, int>(list, numberOfPages);
        }

        public async Task<IEntryList> ReadListAsync(Expression<Func<IEntryModel, bool>> filter, int userId, EntryType entryType, bool orderByDate = false)
        {
            await UpdateRecurringAsync(userId, entryType);
            var updatedFilter = filter.And(x => x.UserId == userId);
            var list = new EntryList(entryType);
            IEnumerable<IEntry> temp = entryType switch
            {
                EntryType.Income => await Database.Incomes.Where(updatedFilter).Select(dbEntry => new Entry(dbEntry) as IEntry).ToListAsync(),
                _ => await Database.Expenses.Where(updatedFilter).Select(dbEntry => new Entry(dbEntry) as IEntry).ToListAsync()
            };

            list.AddRange(orderByDate? temp.OrderByDescending(x => x.Date) : temp);
            return list;
        }

        private async Task UpdateRecurringAsync(int userId, EntryType entryType)
        {
            var list = new EntryList(entryType);
            IEnumerable<IEntry> temp = entryType switch
            {
                EntryType.Income => await Database.Incomes.Where(x => x.UserId == userId && x.IsMonthly).Select(dbEntry => new Entry(dbEntry) as IEntry).ToListAsync(),
                _ => await Database.Expenses.Where(x => x.UserId == userId && x.IsMonthly).Select(dbEntry => new Entry(dbEntry) as IEntry).ToListAsync()
            };
            list.AddRange(temp);
            foreach (var entry in list.Where(entry => !TimeManager.IsDateThisMonthAndLater(entry.Date)))
            {
                await UpdateAsync(entry.Id, userId, entry, entryType);
            }
        }
    }
}
