using System;
using System.Collections.Generic;

namespace CurrencyConverter.Contracts.Outgoing
{
    public class CurrencyDto
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public IEnumerable<int> Symbol { get; set; }

        public decimal Rate { get; set; }
    }
}
