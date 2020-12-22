using System;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement
{
    public static class RecurringUpdater
    {
        public static IEntryList CreateRecurringListWithoutOriginalEntry(IEntry entry, EntryType entryType)
        {
            var tempList = new EntryList(entryType);
            if (TimeManager.IsDateThisMonthAndLater(entry.Date))
            {
                return tempList;
            }
            var month = entry.Date;
            var months = TimeManager.DifferenceInMonths(laterTime: DateTime.Today, earlierTime: month);
            // one less since last one has to keep isMonthly = true;
            for (var i = 0; i < months - 1; i++)
            {
                month = TimeManager.MoveToNextMonth(dateTime: month);
                var newEntry = Entry.CreateLocalEntry(entry.Title, entry.Amount, month, false, entry.Importance, entry.Currency);
                tempList.Add(newEntry);
            }

            month = TimeManager.MoveToNextMonth(month);
            var newestEntry = Entry.CreateLocalEntry(entry.Title, entry.Amount, month, true, entry.Importance, entry.Currency);
            tempList.Add(newestEntry);
            return tempList;
        }

        public static IEntryList CreateRecurringList(IEntry entry, EntryType entryType)
        {
            var tempList = new EntryList(entryType) { entry };
            if (TimeManager.IsDateThisMonthAndLater(entry.Date))
            {
                return tempList;
            }
            entry.Recurring = false;
            tempList.AddRange(CreateRecurringListWithoutOriginalEntry(entry, entryType));
            return tempList;
        }
    }
}
