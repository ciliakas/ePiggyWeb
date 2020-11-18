using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement.Goals;
using HtmlAgilityPack;
namespace ePiggyWeb.Utilities
{
    public class InternetParser
    {
        private readonly Func<int, string, bool> _isTooLong = (x, s) => s.Length > x;
        private readonly Func<decimal, decimal> _convertToEur = (price) => price * (decimal) 1.11;
        private readonly HttpClient _httpClient;
        private delegate decimal Cnv(decimal num);
        private readonly Cnv _cnv;

        public InternetParser(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _cnv = _convertToEur.Invoke;
        }
        public async Task<IGoal> ReadPriceFromCamel(string itemName)
        {
            itemName = WebUtility.UrlEncode(itemName);
            var url = "https://uk.camelcamelcamel.com/search?sq=" + itemName;
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

            var html =  await _httpClient.GetStringAsync(url);

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
                return Goal.CreateLocalGoal(itemName, 0);
            }

            try
            {
                var decimalPrice = Convert.ToDecimal(stringPrice, System.Globalization.CultureInfo.InvariantCulture);
                decimalPrice = _cnv(decimalPrice);
                if (_isTooLong(30, name))
                {
                    itemName = WebUtility.UrlDecode(itemName);
                    name = itemName;
                }

                var temp = Goal.CreateLocalGoal(name, decimalPrice);
                return temp;
            }
            catch(Exception ex)
            {
                ExceptionHandler.Log(ex.ToString());
                itemName = WebUtility.UrlDecode(itemName);
                return Goal.CreateLocalGoal(itemName, 0);
            }
            
        }
    }
}
