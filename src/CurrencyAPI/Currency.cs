﻿using System.Collections.Generic;
using System.Text;

namespace ePiggyWeb.CurrencyAPI
{
    public class Currency
    {
        public static string DefaultCurrencyCode => "EUR";
        public string Name { get; set; }

        public string Code { get; set; }

        private IEnumerable<int> _symbol;

        public IEnumerable<int> Symbol
        {
            get => _symbol;
            set
            {
                _symbol = value;
                SymbolString = GetSymbol();
            }
        }

        public decimal Rate { get; set; }

        public string SymbolString { get; set; }

        public string GetSymbol()
        {
            var sb = new StringBuilder();

            foreach (var symbol in Symbol)
            {
                sb.Append(char.ConvertFromUtf32(symbol));
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var property in GetType().GetProperties())
            {
                var value = property.GetValue(this, null);
                var name = property.Name;
                sb.Append(name);
                sb.Append(": ");
                switch (value)
                {
                    case IEnumerable<int> array:
                        foreach (var symbol in array)
                        {
                            sb.Append(char.ConvertFromUtf32(symbol));
                        }
                        break;
                    default:
                        sb.Append(value);
                        break;
                }
                sb.Append(" ");
            }

            return sb.ToString();
        }
    }
}
