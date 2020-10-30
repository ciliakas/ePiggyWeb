using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement
{
    public class DataEntries : List<DataEntry>
    {
        public EntryType EntryType { get; }

        public DataEntries(EntryType entryType)
        {
            EntryType = entryType;

            var a = this.AsEnumerable();
        }


        //public new IEnumerable<DataEntry> AsEnumerable()
        //{
        //    var a = new List<DataEntry>()
        //    {

        //        new DataEntry(100M, "vienas", DateTime.Today, false, 2),
        //        new DataEntry(200M, "du", DateTime.Today, false, 4)
        //    };
        //    return a.AsEnumerable();
        //}

        public new bool Add(DataEntry dataEntry)
        {
            return false;
        }

        public bool AddRange(DataEntries dataEntries)
        {
            return false;
        }

        public bool Edit(DataEntry oldDataEntry, DataEntry newDataEntry)
        {
            return false;
        }

        public new bool Remove(DataEntry dataEntry)
        {
            return false;
        }

        public bool RemoveList(DataEntries dataEntries)
        {
            RemoveAll(dataEntries.Contains);
            // DATABASE
            // ERROR CHECKING
            return false;
        }
    }
}
