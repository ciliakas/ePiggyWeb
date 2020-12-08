using System;

namespace CurrencyConverter.Contracts.Outgoing
{
    public class CurrencyDto
    {
        public string Code { get; set; }

        public string Symbol { get; set; }

        public decimal Rate { get; set; }
    }
}
