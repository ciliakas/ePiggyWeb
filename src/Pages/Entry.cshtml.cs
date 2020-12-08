using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
        class Rate
        {
            public string Currency { get; set; }
            public decimal Value { get; set; }
        }

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
            var rates = new List<Rate>();

            var doc = new XmlDocument();
            doc.Load(@"http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml");
            // currency symbols: https://gist.githubusercontent.com/bzerangue/5484121/raw/9ded4c5e91a395c76d4a5ccbe8688272c7ea0ed6/currency-symbols.xml

            var nodes = doc.SelectNodes("//*[@currency]");

            if (nodes != null)
            {
                rates.AddRange(from XmlNode node in nodes where node.Attributes != null select new Rate() {Currency = node.Attributes["currency"].Value, Value = decimal.Parse(node.Attributes["rate"].Value, NumberStyles.Any, new CultureInfo("en-Us"))});
            }

            var stb = new StringBuilder();
            foreach (var rate in rates)
            {
                stb.AppendLine(rate.Currency).Append(rate.Value);
            }

            var a = Char.ConvertFromUtf32(76);

            //var symbol = ChrW(&H20B1);

            @ViewData["IncomeList"] = a;
        }
    }
}
