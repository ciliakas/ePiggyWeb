using System;

namespace CurrencyConverter.Contracts.Outgoing
{
    public class CurrencyDto
    {
        public string CurrencyName { get; set; }

        public char CurrencySymbol { get; set; }

        public DateTime CurrencyDate { get; set; }
    }
}
