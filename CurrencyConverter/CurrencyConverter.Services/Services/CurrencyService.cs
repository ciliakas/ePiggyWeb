using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using CurrencyConverter.Domain;

namespace CurrencyConverter.Services.Services
{
    public class CurrencyService : ICurrencyService
    {
        public Currency GetCurrencyByCode(string code)
        {
            var list = GetCurrencyList();
            var currency = list.FirstOrDefault(x => x.Code == code);
            return currency;
        }

        public IList<Currency> GetCurrencyList()
        {
            IList<Currency> list = new List<Currency> { new Currency { Code = "EUR", Rate = 1, Symbol = "E" } };

            var doc = new XmlDocument();
            doc.Load(@"http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml");

            XmlNodeList nodes = doc.SelectNodes("//*[@currency]");

            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes == null) continue;
                    var rate = new Currency
                    {
                        Code = node.Attributes["currency"].Value,
                        Symbol = "",
                        Rate = decimal.Parse(node.Attributes["rate"].Value, NumberStyles.Any, new CultureInfo("en-Us"))
                    };
                    list.Add(rate);
                }
            }

            return list;
        }

        public decimal? GetCustomRate(string currencyCode1, string currencyCode2)
        {
            var list = GetCurrencyList();

            var currency1 = list.FirstOrDefault(x => x.Code == currencyCode1);
            var currency2 = list.FirstOrDefault(x => x.Code == currencyCode2);

            if (currency1 is null || currency2 is null)
            {
                return null;
            }

            var rate = decimal.Round(currency2.Rate / currency1.Rate,4);

            return rate;
        }
    }
}
