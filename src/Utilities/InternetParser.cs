using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ePiggyWeb.Utilities;
using HtmlAgilityPack;

namespace ePiggy.Utilities
{
    public static class InternetParser
    {
        /*Example of calling method:
         * Task.Run(() => InternetParser.GetHTMLAsync()).Wait();
        */
        private static readonly string ResourceDirectoryParsedGoal = Directory.GetParent(Environment.CurrentDirectory)
                                                                         .Parent?.Parent?.FullName +
                                                                     @"\resources\textData\parsedGoal.txt";
        public static async Task ReadPriceFromCamel(string itemName)
        {
            var HttpClient = new HttpClient();

            itemName = WebUtility.UrlEncode(itemName);
            var url = "https://uk.camelcamelcamel.com/search?sq=" + itemName;/*Need tweaking with symbols inside itemName*/
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

            var html =  await HttpClient.GetStringAsync(url);

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


            try
            {
                await File.WriteAllTextAsync(ResourceDirectoryParsedGoal, name + "\n" + stringPrice);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Log(ex.ToString());
            }
            
            
        }
    }
}
