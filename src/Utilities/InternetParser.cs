using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement.Goals;
using HtmlAgilityPack;
namespace ePiggyWeb.Utilities
{
    public static class InternetParser
    {
        private static readonly Func<int, string, bool> IsTooLong = (int x, string s) => s.Length > x;
        private static readonly Func<decimal, decimal> ConvertToEur = (decimal price) => price * (decimal) 1.11;

        public static async Task<IGoal> ReadPriceFromCamel(string itemName)
        {
            var httpClient = new HttpClient();

            itemName = WebUtility.UrlEncode(itemName);
            var url = "https://uk.camelcamelcamel.com/search?sq=" + itemName;
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

            var html =  await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var productHtml = htmlDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "")/*Everything on the page*/
                .Equals("row column search_results")).ToList();

            var productListItems = productHtml[0].Descendants("div").Where(node => node.GetAttributeValue("class", "")
            .Equals("row")).ToList();

         
            var name = productListItems[0].Descendants("strong").FirstOrDefault()?.InnerText;
            name = name?.Remove(name.Length - 13);

            var stringPrice = productListItems[0].Descendants("span").FirstOrDefault(node => node.GetAttributeValue("class", "")
                    .Equals("green"))
                    ?.InnerText;
            stringPrice = stringPrice?.Substring(1).Trim();
            if (stringPrice == null)
            {
                httpClient.Dispose();
                return Goal.CreateLocalGoal(itemName, 0);
            }

            try
            {
                var decimalPrice = Convert.ToDecimal(stringPrice, System.Globalization.CultureInfo.InvariantCulture);
                decimalPrice = ConvertToEur(decimalPrice);
                if (IsTooLong(30, name))
                {
                    itemName = WebUtility.UrlDecode(itemName);
                    name = itemName;
                }

                var temp = Goal.CreateLocalGoal(name, decimalPrice);
                httpClient.Dispose();
                return temp;
            }
            catch(Exception ex)
            {
                ExceptionHandler.Log(ex.ToString());
                itemName = WebUtility.UrlDecode(itemName);
                httpClient.Dispose();
                return Goal.CreateLocalGoal(itemName, 0);
            }
            
        }
    }
}
