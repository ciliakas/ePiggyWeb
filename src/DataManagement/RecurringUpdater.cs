using System;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement
{
    public static class RecurringUpdater
    {
        public static bool UpdateRecurring(IEntryManager entryManager)
        {
            foreach (var entry in entryManager.EntryList.GetBy(recurring: true))
            {
                var differenceInMonths = TimeManager.DifferenceInMonths(laterTime: DateTime.Today, earlierTime: entry.Date);
                if (differenceInMonths <= 0) continue;
                entry.Date = entry.Date.AddMonths(differenceInMonths);
                entryManager.Edit(entry.Id, entry);
            }
            return true;
        }

        public static IEntryList CreateRecurringListWithoutOriginalEntry(IEntry entry, EntryType entryType)
        {
            var month = entry.Date;
            var months = TimeManager.DifferenceInMonths(laterTime: DateTime.Today, earlierTime: month);
            var tempList = new EntryList(entryType);
            // one less since last one has to keep isMonthly = true;
            for (var i = 0; i < months - 1; i++)
            {
                month = TimeManager.MoveToNextMonth(dateTime: month);
                var newEntry = new Entry(entry.Title, entry.Amount, month, false, entry.Importance);
                tempList.Add(newEntry);
            }

            month = TimeManager.MoveToNextMonth(month);
            var newestEntry = new Entry(entry.Title, entry.Amount, month, true, entry.Importance);
            tempList.Add(newestEntry);
            return tempList;
        }

        public static IEntryList CreateRecurringList(IEntry entry, EntryType entryType)
        {
            entry.Recurring = false;
            var tempList = new EntryList(entryType) { entry };
            tempList.AddRange(CreateRecurringListWithoutOriginalEntry(entry, entryType));
            return tempList;
        }
    }
}
