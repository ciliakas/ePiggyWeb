using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ePiggyWeb.CurrencyAPI
{
    public class CurrencyApiAgent
    {
        private const string CurrencyApi = "CurrencyAPI";
        private const string ApiRoute = "APIRoute";
        private const string ListRoute = "list";
        private const string CurrencyRoute = "currency";
        private HttpClient HttpClient { get; }
        private string ConnectionString { get; }

        public CurrencyApiAgent(HttpClient httpClient, IConfiguration configuration)
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

        public async Task<IList<Currency>> GetList()
        {
            var message = await SendRequest(ConnectionString + ListRoute);
            var listDto = JsonConvert.DeserializeObject<List<CurrencyDto>>(message);
            IList<Currency> list = listDto.Select(dto => dto.FromDto()).ToList();
            return list;
        }

        private async Task<string> SendRequest(string route)
        {
            var response = await HttpClient.GetAsync(route);
            if (!response.IsSuccessStatusCode) throw new HttpListenerException();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
