using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntryList : IList<IEntry>, IEntryEnumerable
    {

    }
}
