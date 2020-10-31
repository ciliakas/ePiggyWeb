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
        private EntryList EntryList { get; }

        public EntryManager(EntryList entryListList)
        {
            EntryList = entryListList;
        }

        public bool Add(Entry entry)
        {
            throw new Exception("Not implemented");
        }

        public bool AddRange(EntryList entryList)
        {
            //base.AddRange(entryList);
            //DB
            throw new Exception("Not implemented");
        }

        public bool Edit(Entry oldEntry, Entry newEntry)
        {
            throw new Exception("Not implemented");
        }

        public bool Remove(Entry entry)
        {

            //DB
            //return base.Remove(entry);
            throw new Exception("Not implemented");
        }

        public bool RemoveList(EntryList entryList)
        {
            //RemoveAll(entryList.Contains);
            // DATABASE
            // ERROR CHECKING
            throw new Exception("Not implemented");
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
