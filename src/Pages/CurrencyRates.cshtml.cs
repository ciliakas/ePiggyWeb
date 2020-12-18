using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace ePiggyWeb.Pages
{
    public class CurrencyRatesModel : PageModel
    {
        private HttpClient HttpClient { get; }
        private IConfiguration Configuration { get; }
        public IList<Currency> Currencies { get; set; }

        public CurrencyRatesModel(HttpClient httpClient, IConfiguration configuration)
        {
            HttpClient = httpClient;
            Configuration = configuration;
        }
        public async Task OnGet()
        {
            var currencyConverter = new CurrencyApiAgent(HttpClient, Configuration);
            Currencies = await currencyConverter.GetList();
        }
    }
}
