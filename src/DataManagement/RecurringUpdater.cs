﻿using System;
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
            var tempList = new EntryList(entryType);
            if ((entry.Date.Year == DateTime.UtcNow.Year && entry.Date.Month >= DateTime.UtcNow.Month) || entry.Date.Year > DateTime.UtcNow.Year)
            {
                return tempList;
            }
            var month = entry.Date;
            var months = TimeManager.DifferenceInMonths(laterTime: DateTime.Today, earlierTime: month);
            // one less since last one has to keep isMonthly = true;
            for (var i = 0; i < months - 1; i++)
            {
                month = TimeManager.MoveToNextMonth(dateTime: month);
                var newEntry = Entry.CreateLocalEntry(entry.Title, entry.Amount, month, false, entry.Importance);
                tempList.Add(newEntry);
            }

            month = TimeManager.MoveToNextMonth(month);
            var newestEntry = Entry.CreateLocalEntry(entry.Title, entry.Amount, month, true, entry.Importance);
            tempList.Add(newestEntry);
            return tempList;
        }

        public static IEntryList CreateRecurringList(IEntry entry, EntryType entryType)
        {
            var tempList = new EntryList(entryType) { entry };
            if ((entry.Date.Year == DateTime.UtcNow.Year && entry.Date.Month >= DateTime.UtcNow.Month) || entry.Date.Year > DateTime.UtcNow.Year)
            {
                return tempList;
            }
            entry.Recurring = false;
            tempList.AddRange(CreateRecurringListWithoutOriginalEntry(entry, entryType));
            return tempList;
        }
    }
}
