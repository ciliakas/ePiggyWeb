using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntryEnumerable : IEnumerable<IEntry>
    {
        public EntryType EntryType { get; set; }
    }
}
