using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ePiggyWeb.Utilities;
using System.Diagnostics.CodeAnalysis;
using ePiggyWeb.CurrencyAPI;
using Microsoft.Extensions.Configuration;

namespace ePiggyWeb.DataManagement.Entries
{
    public class EntryList : List<IEntry>, IEntryList
    {
        public EntryType EntryType { get; set; }
        public EntryList(EntryType entryType)
        {
            EntryType = entryType;
        }

        public EntryList(IEnumerable<IEntry> entryList)
        {
            if (entryList is null) throw new ArgumentException();
            AddRange(entryList);
        }

        private EntryList(EntryType entryType, IEnumerable<IEntry> entryList)
        {
            EntryType = entryType;
            if (entryList is null) throw new ArgumentException();
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

        public IEntryList GetPage(int pageNumber, int pageSize)
        {
            return new EntryList(EntryType, this.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
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

            foreach (var configurationSection in section.GetChildren())
            {
                var amount = random.Next(50, 500);
                var importance = random.Next(minImportance, maxImportance);
                IEntry entry = Entry.CreateLocalEntry(configurationSection.Value, amount, 
                    DateTime.UtcNow, recurring: false, importance, currency: Currency.DefaultCurrencyCode);
                list.Add(entry);
            }

            return list;
        }
    }
}
