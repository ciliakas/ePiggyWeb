using System;
using System.Data;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;

namespace ePiggyWeb.Utilities
{
    public class DataTableConverter
    {
        private DataManager DataManager { get; }
        public DataTableConverter(DataManager dataManager)
        {
            DataManager = dataManager;
        }

        //public DataTable GenerateSuggestionTable(List<EntrySuggestion> entrySuggestions)
        //{
        //    var dt = GenerateSuggestionTableHeaders();

        //    foreach (var dataOffer in entrySuggestions)
        //    {
        //        dt.Rows.Add(dataOffer.Entry.Id, dataOffer.Entry.Title, dataOffer.Entry.Amount,
        //            dataOffer.Entry.Date, dataOffer.Entry.Importance, dataOffer.Entry.IsMonthly, dataOffer.Amount);
        //    }

        //    return dt;
        //}

        private static DataTable GenerateSuggestionTableHeaders()
        {
            var dt = GenerateEntryTableHeaders();
            dt.Columns.Add("Suggested Amount", typeof(decimal));
            return dt;
        }

        public DataTable GenerateEntryTable(EntryType entryType)//All entries
        {
            var dt = GenerateEntryTableHeaders();
            var dataEntries = entryType == EntryType.Expense ? DataManager.Expenses.EntryList : DataManager.Income.EntryList;
            foreach (var data in dataEntries)
            {
                dt.Rows.Add(data.Id, data.Title, data.Amount, data.Date, data.Importance, data.IsMonthly);
            }
            return dt;
        }

        public static DataTable GenerateEntryTable(EntryList entryList)
        {
            var dt = GenerateEntryTableHeaders();
            foreach (var entry in entryList)
            {
                dt.Rows.Add(entry.Id, entry.Title, entry.Amount, entry.Date, entry.Importance, entry.IsMonthly);
            }
            return dt;
        }

        private static DataTable GenerateEntryTableHeaders()
        {
            var dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Title", typeof(string));
            dt.Columns.Add("Amount", typeof(decimal));
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Importance", typeof(Importance));
            dt.Columns.Add("Recurring", typeof(bool));
            return dt;
        }
    }
}
