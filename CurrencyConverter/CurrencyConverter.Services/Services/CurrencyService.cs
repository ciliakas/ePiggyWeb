using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

        private IEnumerable<Currency> GetECBList()
        {
            IList<Currency> list = new List<Currency> { new Currency { Code = "EUR", Rate = 1 } };
            var doc = new XmlDocument();
            doc.Load(@"http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml");
            var nodes = doc.SelectNodes("//*[@currency]");

            if (nodes is null)
            {
                return list;
            }

            foreach (XmlNode node in nodes)
            {
                if (node.Attributes is null) continue;
                var rate = new Currency
                {
                    Code = node.Attributes["currency"].Value,
                    Rate = decimal.Parse(node.Attributes["rate"].Value, NumberStyles.Any, new CultureInfo("en-Us"))
                };
                list.Add(rate);
            }

            return list.ToArray();
        }

        public IEnumerable<Currency> GetCurrencyList()
        {
            var ecbList = GetECBList();
            var symbolList = GetCurrencySymbolList();

            var list =
                from ecbCurrency in ecbList
                join symbolCurrency in symbolList on ecbCurrency.Code equals symbolCurrency.Code
                select new Currency
                {
                    Name = symbolCurrency.Name,
                    Code = symbolCurrency.Code,
                    Symbol = symbolCurrency.Symbol,
                    Rate = ecbCurrency.Rate
                };

            return list;
        }

        public decimal? GetCustomRate(string currencyCode1, string currencyCode2)
        {
            var list = GetECBList();

            var currencies = list as Currency[] ?? list.ToArray();
            var currency1 = currencies.FirstOrDefault(x => x.Code == currencyCode1);
            var currency2 = currencies.FirstOrDefault(x => x.Code == currencyCode2);

            if (currency1 is null || currency2 is null)
            {
                return null;
            }

            var rate = currency2.Rate / currency1.Rate;

            return rate;
        }


        public IList<Currency> GetCurrencySymbolList()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var doc = new XmlDocument();
            using var stream = assembly.GetManifestResourceStream("CurrencyConverter.Services.SymbolSheet.xml");
            doc.Load(stream ?? throw new InvalidOperationException());

            var nodes = doc.SelectNodes("//*[@code]");
            IList<Currency> list = new List<Currency>();

            if (nodes is null)
            {
                return list;
            }

            foreach (XmlNode node in nodes)
            {
                if (node.Attributes is null) continue;
                var symbolString = node.Attributes["unicode-decimal"].Value.Split(',');
                IList<int> symbol = new List<int>();
                foreach (var str in symbolString)
                {
                    if (int.TryParse(str, out var tempStr))
                    {
                        symbol.Add(tempStr);
                    }
                }
                var rate = new Currency { Name = node.InnerText, Code = node.Attributes["code"].Value, Symbol = symbol };
                list.Add(rate);
            }

            return list;
        }
    }
}
