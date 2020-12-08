using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using CurrencyConverter.Domain;

namespace CurrencyConverter.Services.Services
{
    public class CurrencyService : ICurrencyService
    {
        public Currency GetCurrencyByCode(string code)
        {
            throw new NotImplementedException();
        }

        public IList<Currency> GetCurrencyList()
        {

            IList<Currency> list = new List<Currency>();

            var doc = new XmlDocument();
            doc.Load(@"http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml");

            XmlNodeList nodes = doc.SelectNodes("//*[@currency]");

            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
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

        public decimal GetCustomRate(string currencyCode1, string currencyCode2)
        {
            throw new NotImplementedException();
        }
    }
}
