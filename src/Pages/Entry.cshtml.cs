using System.Net.Http;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ePiggyWeb.Pages
{
    public class EntryModel : PageModel
    {
        private EntryDatabase EntryDatabase { get; }
        private GoalDatabase GoalDatabase { get; }
        private UserDatabase UserDatabase { get; }
        private EmailSender EmailSender { get; }
        private IConfiguration Configuration { get; }
        private int UserId { get; set; }

        private HttpClient HttpClient { get; }

        public EntryModel(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task OnGet()
        {
            //@ViewData["EntryList"] = EntryList.RandomList(Configuration, EntryType.Expense);
            //Http
            var responseBodyString = await HttpClient.GetStringAsync("https://localhost:44392/currency");
            @ViewData["IncomeList"] = responseBodyString;

        }
    }
}
