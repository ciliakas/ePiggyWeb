using System.Collections.Generic;
using System.Text;
using ePiggyWeb.Utilities;

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
