using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ePiggyWeb.CurrencyAPI
{
    public class CurrencyConverter
    {
        private const string CurrencyApi = "CurrencyAPI";
        private const string ApiRoute = "APIRoute";
        private const string ListRoute = "list";
        private const string CurrencyRoute = "currency";
        private const string RateRoute = "rate";
        private const string SymbolsRoute = "symbols";
        private HttpClient HttpClient { get; set; }
        private string ConnectionString { get; }

        public CurrencyConverter(HttpClient httpClient, IConfiguration configuration)
        {
            HttpClient = httpClient;
            ConnectionString = configuration.GetSection(CurrencyApi).GetSection(ApiRoute).Value;
        }

        public async Task<Currency> GetCurrency(string code)
        {
            var message = await SendRequest(ConnectionString + CurrencyRoute + "?code=" + code);
            var currencyDto = JsonConvert.DeserializeObject<CurrencyDto>(message);
            var currency = currencyDto.FromDto();
            return currency;
        }


        public async Task<string> GetCurrencySymbol(string code)
        {
            Currency currency;
            try
            {
                currency = await GetCurrency(code);
            }
            catch (Exception)
            {
                return code;
            }
            return currency.GetSymbol();
        }


        public async Task<IList<Currency>> GetList()
        {
            var message = await SendRequest(ConnectionString + ListRoute);
            var listDto = JsonConvert.DeserializeObject<List<CurrencyDto>>(message);
            IList<Currency> list = listDto.Select(dto => dto.FromDto()).ToList();
            return list;
        }

        public async Task<decimal> GetRate(string currencyFrom, string currencyTo)
        {
            var message = await SendRequest(ConnectionString + RateRoute + "?currencyCode1=" + currencyFrom + "&currencyCode2=" + currencyTo);
            var rate = JsonConvert.DeserializeObject<decimal>(message);
            return rate;
        }

        public async Task<IList<Currency>> GetSymbols()
        {
            var message = await SendRequest(ConnectionString + SymbolsRoute);
            var listDto = JsonConvert.DeserializeObject<List<CurrencyDto>>(message);
            IList<Currency> list = listDto.Select(dto => dto.FromDto()).ToList();
            return list;
        }

        private async Task<string> SendRequest(string route)
        {
            var response = await HttpClient.GetAsync(route);
            if (!response.IsSuccessStatusCode) throw new Exception();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
