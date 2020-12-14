using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ePiggyWeb.Pages
{
    public class EntryModel : PageModel
    {

        private HttpClient HttpClient { get; }
        private IConfiguration Configuration { get; }

        public EntryModel(HttpClient httpClient, IConfiguration configuration)
        {
            HttpClient = httpClient;
            Configuration = configuration;
        }

        public async Task OnGet()
        {
            var currencyConverter = new CurrencyConverter(HttpClient, Configuration);
            var list = await currencyConverter.GetList();
            var sb = new StringBuilder();

            foreach (var currency in list)
            {
                sb.AppendLine(currency.ToString());
            }

            @ViewData["IncomeList"] = sb.ToString();
        }
    }
}
