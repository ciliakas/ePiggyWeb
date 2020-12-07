using System;
using CurrencyConverter.Domain;


namespace CurrencyConverter.Services.Services
{
    public class CurrencyService : ICurrencyService
    {
        public Currency GetCurrencyByName(string firstName, string lastName)
        {
            return new Currency() {CurrencyName = "TOP", CurrencySymbol = '$', CurrencyDate = DateTime.UtcNow};
        }
    }
}
