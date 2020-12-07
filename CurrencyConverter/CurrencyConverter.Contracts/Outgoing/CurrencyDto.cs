using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyConverter.Contracts.Outgoing
{
    public class CurrencyDto
    {
        public string CurrencyName { get; set; }

        public char CurrencySymbol { get; set; }

        public DateTime CurrencyDate { get; set; }
    }
}
