using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ePiggyWeb.DataBase
{
    public class EntryDb
    {
        private PiggyDbContext Database { get; }
        public EntryDb(PiggyDbContext database)
        {
            Database = database;
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

        public async Task<IEnumerable<IEntry>> ReadList(int userId, EntryType entryType)
        {
            return await ReadList(x => x.UserId == userId, entryType);
        }

        public async Task<IEnumerable<IEntry>> ReadList(Expression<Func<IEntryModel, bool>> filter, EntryType entryType)
        {
            IEnumerable<IEntry> temp = entryType switch
            {
                EntryType.Income => await Database.Incomes.Where(filter).Select(dbEntry => new Entry(dbEntry)).Cast<IEntry>().ToListAsync(),
                _ => await Database.Expenses.Where(filter).Select(dbEntry => new Entry(dbEntry)).Cast<IEntry>().ToListAsync()
            };
            return temp;
        }
    }
}
