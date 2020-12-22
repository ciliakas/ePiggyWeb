using System.Collections.Generic;
using CurrencyConverter.Domain;

namespace CurrencyConverter.Services.Services
{
    public interface ICurrencyService
    {
        public Currency GetCurrencyByCode(string code);

        public IEnumerable<Currency> GetCurrencyList();

        public decimal? GetCustomRate(string currencyCode1, string currencyCode2);

        public IList<Currency> GetCurrencySymbolList();
    }
}
