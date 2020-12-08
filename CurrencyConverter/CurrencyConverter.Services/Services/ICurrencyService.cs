using CurrencyConverter.Domain;

namespace CurrencyConverter.Services.Services
{
    public interface ICurrencyService
    {
        public Currency GetCurrencyByName(string firstName, string lastName);

        //public 
    }
}
