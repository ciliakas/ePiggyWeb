using System;
using System.Collections.Generic;

namespace CurrencyConverter.Domain
{
    public class Currency
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public IEnumerable<int> Symbol { get; set; }

        public decimal Rate { get; set; }
    }
}
