using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ePiggyWeb.Utilities;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace ePiggyWeb.DataManagement.Entries
{
    public class EntryList : List<IEntry>, IEntryList
    {
        //I added EntryType to EntryList so we don't have to pass an EntryType as a parameter in methods as often
        public EntryType EntryType { get; set; }
        public EntryList(EntryType entryType)
        {
            EntryType = entryType;
        }

        private EntryList(EntryType entryType, IEnumerable<IEntry> entryList)
        {
            EntryType = entryType;
            if (entryList is null) return;
            AddRange(entryList);
        }
        
        public IEntryList GetBy(Importance importance)
        {
            return new EntryList(EntryType, this.Where(x => x.Importance == (int)importance).ToList());
        }

        public IEntryList GetBy(DateTime dateTime)
        {
            return new EntryList(EntryType, this.Where(x => x.Date.Year == dateTime.Year && x.Date.Month == dateTime.Month).ToList());
        }

        public IEntryList GetFrom(DateTime dateTime)
        {
            return new EntryList(EntryType, this.Where(x => x.Date >= dateTime).ToList());
        }

        public IEntryList GetTo(DateTime dateTime)
        {
            return new EntryList(EntryType, this.Where(x => x.Date <= dateTime).ToList());
        }

        public IEntryList GetBy(bool recurring)
        {
            return new EntryList(EntryType, this.Where(x => x.Recurring == recurring).ToList());
        }

        public IEntryList GetUntilToday()
        {
            return new EntryList(EntryType, this.Where(x => x.Date <= DateTime.Today).ToList());
        }

        public DateTime GetOldestEntryDate()
        {
            return this.Min(x => x.Date);
        }

        public DateTime GetNewestEntryDate()
        {
            return this.Max(x => x.Date);
        }

        public decimal GetSum()
        {
            return this.Sum(entry => entry.Amount);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var entry in this)
            {
                sb.Append(entry);
                //sb.Append("\n");
            }

            return sb.ToString();
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static IEntryList RandomList(IConfiguration configuration, EntryType entryType)
        {
            IEntryList list = new EntryList(entryType);
            var section = configuration.GetSection(entryType.ToString());
            var random = new Random();
            var enumCount = Enum.GetValues(typeof(Importance));

            var minImportance = (int)enumCount.GetValue(0);
            var maxImportance = (int)enumCount.GetValue(enumCount.Length - 1);


            foreach (var thing in section.GetChildren())
            {
                var amount = random.Next(50, 500);
                var importance = random.Next(minImportance, maxImportance);
                IEntry entry = Entry.CreateLocalEntry(thing.Value, amount, DateTime.UtcNow, false, importance);
                list.Add(entry);
            }

            return list;
        }
    }
}
