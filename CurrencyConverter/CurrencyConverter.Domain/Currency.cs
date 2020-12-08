using System;

namespace CurrencyConverter.Domain
{
    public class Currency
    {
        public string Code { get; set; }

        public string Symbol { get; set; }

        public decimal Rate { get; set; }
    }
}
