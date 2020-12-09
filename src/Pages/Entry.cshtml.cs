using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace ePiggyWeb.Pages
{
    public class EntryModel : PageModel
    {

        private HttpClient HttpClient { get; }

        public EntryModel(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task OnGet()
        {
            var currencyConverter = new CurrencyConverter(HttpClient);
            var list = await currencyConverter.GetList(); 
            var sb = new StringBuilder();

            foreach (var currency in list)
            {
                sb.AppendLine(currency.GetSymbol());
            }

            @ViewData["IncomeList"] = sb.ToString();
        }
    }
}
