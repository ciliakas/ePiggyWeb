using System.Collections.Generic;
using CurrencyConverter.Domain;

namespace CurrencyConverter.Services.Services
{
    public interface ICurrencyService
    {
        public Currency GetCurrencyByCode(string code);

        public IList<Currency> GetCurrencyList();

        public decimal? GetCustomRate(string currencyCode1, string currencyCode2);

        //public 
    }
}
