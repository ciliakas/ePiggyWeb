using CurrencyConverter.Contracts.Outgoing;
using CurrencyConverter.Domain;

namespace CurrencyConverter.Services.Mapper
{
    public static class CurrencyMapper
    {
        public static CurrencyDto ToDto(this Currency from)
        {
            return new CurrencyDto { CurrencyName = from.CurrencyName, CurrencySymbol = from.CurrencySymbol, CurrencyDate = from.CurrencyDate };
        }
    }

}
