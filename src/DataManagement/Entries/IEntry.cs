using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool Recurring { get; set; }
        public int Importance { get; set; }
        public void Edit(IEntry newEntry);
        public int CompareTo(IFinanceable other);
        public int CompareTo(IEntry other);
        public bool Equals(IFinanceable other);
        public bool Equals(IEntry other);
    }
}
