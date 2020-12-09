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

            // currency symbols: https://gist.githubusercontent.com/bzerangue/5484121/raw/9ded4c5e91a395c76d4a5ccbe8688272c7ea0ed6/currency-symbols.xml


            IList<string> list = new List<string>();

            var doc = new XmlDocument();
            doc.Load(@"https://gist.githubusercontent.com/bzerangue/5484121/raw/9ded4c5e91a395c76d4a5ccbe8688272c7ea0ed6/currency-symbols.xml");
            XmlNodeList nodes = doc.SelectNodes("//*[@code]");

            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes == null) continue;
                    list.Add(node.Attributes["code"].Value + " " + node.Attributes["unicode-decimal"].Value + " " + node.InnerText);
                }
            }


            var stb = new StringBuilder();
            foreach (var str in list)
            {
                stb.AppendLine(str);
            }

            var a = Char.ConvertFromUtf32(8378);

            //var symbol = ChrW(&H20B1);

            @ViewData["IncomeList"] = a.ToString();
        }
    }
}
