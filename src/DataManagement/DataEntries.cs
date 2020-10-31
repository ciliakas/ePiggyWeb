using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement
{
    public class DataEntries : List<DataEntry>
    {
        public EntryType EntryType { get; set; }

        public DataEntries() { }

        public DataEntries(EntryType entryType)
        {
            EntryType = entryType;
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

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var dataEntry in this)
            {
                sb.Append(dataEntry);
                //sb.Append("\n");
            }

            return sb.ToString();
        }

        public new bool Add(DataEntry dataEntry)
        {
            base.Add(dataEntry);
            //DB
            return true;
        }

        public bool AddRange(DataEntries dataEntries)
        {
            base.AddRange(dataEntries);
            //DB
            return true;
        }

        public bool Edit(DataEntry oldDataEntry, DataEntry newDataEntry)
        {
            throw new Exception("Not implemented");
        }

        public new bool Remove(DataEntry dataEntry)
        {

            //DB
            return base.Remove(dataEntry);
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
