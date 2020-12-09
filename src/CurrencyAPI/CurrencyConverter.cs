using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ePiggyWeb.CurrencyAPI
{
    public class CurrencyConverter
    {
        private HttpClient HttpClient { get; set; }

        public CurrencyConverter(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<IList<Currency>> GetList()
        {
            var response = await HttpClient.GetAsync("https://localhost:44392/list");

            if(!response.IsSuccessStatusCode) throw new Exception();

            var message = await response.Content.ReadAsStringAsync();

            var listDto = JsonConvert.DeserializeObject<List<CurrencyDto>>(message);

            IList<Currency> list = listDto.Select(dto => dto.FromDto()).ToList();

            return list;
        }

    }
}
