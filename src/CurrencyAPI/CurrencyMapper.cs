namespace ePiggyWeb.CurrencyAPI
{
    public static class CurrencyMapper
    {
        public static CurrencyDto ToDto(this Currency from)
        {
            return new CurrencyDto { Name = from.Name, Code = from.Code, Symbol = from.Symbol, Rate = from.Rate };
        }

        public static Currency FromDto(this CurrencyDto from)
        {
            return new Currency { Name = from.Name, Code = from.Code, Symbol = from.Symbol, Rate = from.Rate };
        }
    }
}
