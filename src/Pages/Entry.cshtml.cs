using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
            var list = await HttpClient.GetStringAsync("https://localhost:44392/list");

            //var ch = char.ConvertFromUtf32(8378);

            //@ViewData["IncomeList"] = ch.ToString();
            @ViewData["IncomeList"] = list;
        }
    }
}
