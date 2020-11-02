using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ePiggyWeb.DataManagement
{
    //Name could use some work
    public interface IFinanceable
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; }

        public decimal Amount { get; set; }
    }
}
