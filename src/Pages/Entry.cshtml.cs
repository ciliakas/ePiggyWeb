using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace ePiggyWeb.Pages
{
    public class EntryModel : PageModel
    {

        public class CurrencyDto
        {
            public string Name { get; set; }

            public string Code { get; set; }

            public IEnumerable<int> Symbol { get; set; }

            public decimal Rate { get; set; }
        }

        private HttpClient HttpClient { get; }

        public EntryModel(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task OnGet()
        {
            var test = await HttpClient.GetAsync("https://localhost:44392/list");
            var list = await HttpClient.GetStringAsync("https://localhost:44392/list");

            //test.
            var thing = JsonConvert.DeserializeObject<List<CurrencyDto>>(list);

            var thingy = thing.First();

            //test.Content.

            //var ch = char.ConvertFromUtf32(8378);

            //@ViewData["IncomeList"] = ch.ToString();
            var str = thingy.Name + " " + thingy.Code + " " + thingy.Rate + " " + thingy.Symbol;

            @ViewData["IncomeList"] = str;
        }
    }
}
