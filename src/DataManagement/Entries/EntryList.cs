using System.Collections.Generic;
using System.Text;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
    public class EntryList : List<IEntry>
    {
        //EntryType is to save up on the amount of times we have to pass the EntryType in methods
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
