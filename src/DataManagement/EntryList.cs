using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement
{
    public class EntryList : List<Entry>
    {
        public EntryType EntryType { get; set; }

        public EntryList() { }

        public EntryList(EntryType entryType)
        {
            EntryType = entryType;
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
    }
}
