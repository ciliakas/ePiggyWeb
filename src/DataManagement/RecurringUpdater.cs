using System;
using ePiggyWeb.DataBase;
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
                if (!UpdateEntry(entryManager, entry, differenceInMonths))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool UpdateEntry(IEntryManager entryManager, IEntry entry, int months)
        {
            var nextMonth = entry.Date;
            // one less since last one has to keep isMonthly = true;
            var tempList = new EntryList(entryManager.EntryList.EntryType);
            for (var i = 0; i < months - 1; i++)
            {
                //Adding new entry for each month according to date difference
                nextMonth = TimeManager.MoveToNextMonth(dateTime: nextMonth);
                var newEntry = new Entry(entry.Title, entry.Amount, nextMonth, false, entry.Importance);
                tempList.Add(newEntry);
            }

            /*Adding last entry, which has to keep isMonthly*/
            nextMonth = TimeManager.MoveToNextMonth(nextMonth);
            var newestEntry = new Entry(entry.Title, entry.Amount, nextMonth, true, entry.Importance);
            tempList.Add(newestEntry);
            if (!entryManager.AddRange(tempList))
            {
                ExceptionHandler.Log("Couldn't add list of entries in updater: " + tempList);
                return false;
            }

            //Moved here since no point to edit in each cycle rotation
            var editedOriginalEntry = new Entry(entry.Id, entry.UserId, entry.Title, entry.Amount, entry.Date, false, entry.Importance);
            if (entryManager.Edit(entry, editedOriginalEntry))
            {
                return true;
            }

            ExceptionHandler.Log("Couldn't edit entry in updater:" + editedOriginalEntry);
            return false;
        }

        public static bool UpdateEntry(IEntry entry, int userId, EntryType entryType)
        {
            var nextMonth = entry.Date;
            var months = TimeManager.DifferenceInMonths(laterTime: DateTime.Today, earlierTime: nextMonth);
            // one less since last one has to keep isMonthly = true;
            var tempList = new EntryList(entryType);
            for (var i = 0; i < months - 1; i++)
            {
                //Adding new entry for each month according to date difference
                nextMonth = TimeManager.MoveToNextMonth(dateTime: nextMonth);
                var newEntry = new Entry(entry.Title, entry.Amount, nextMonth, false, entry.Importance);
                tempList.Add(newEntry);
            }

            /*Adding last entry, which has to keep isMonthly*/
            nextMonth = TimeManager.MoveToNextMonth(nextMonth);
            var newestEntry = new Entry(entry.Title, entry.Amount, nextMonth, true, entry.Importance);
            tempList.Add(newestEntry);
            if (!EntryDatabase.CreateList(tempList, userId))
            {
                ExceptionHandler.Log("Couldn't add list of entries in updater: " + tempList);
                return false;
            }

            //Moved here since no point to edit in each cycle rotation
            var editedOriginalEntry = new Entry(entry.Id, entry.UserId, entry.Title, entry.Amount, entry.Date, false, entry.Importance);
            if (EntryDatabase.Update(entry.Id, userId, editedOriginalEntry, entryType))
            {
                return true;
            }

            ExceptionHandler.Log("Couldn't edit entry in updater:" + editedOriginalEntry);
            return false;
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

        public static bool AddMonthlyEntry(IEntry entry, int userId, EntryType entryType)
        {
            var tempList = CreateRecurringList(entry, entryType);
            if (EntryDatabase.CreateList(tempList, userId))
            {
                return true;
            }
            ExceptionHandler.Log("Couldn't add list of entries in updater: " + tempList);
            return false;
        }
    }
}
