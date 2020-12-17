using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ePiggyWeb.Pages
{
    public static class CacheKeys
    {
        public static string Entry { get { return "_Entry"; } }
        public static string CallbackEntry { get { return "_Callback"; } }
        public static string CallbackMessage { get { return "_CallbackMessage"; } }
        public static string Parent { get { return "_Parent"; } }
        public static string Child { get { return "_Child"; } }
        public static string DependentMessage { get { return "_DependentMessage"; } }
        public static string DependentCTS { get { return "_DependentCTS"; } }
        public static string Ticks { get { return "_Ticks"; } }
        public static string CancelMsg { get { return "_CancelMsg"; } }
        public static string CancelTokenSource { get { return "_CancelTokenSource"; } }
    }

    public class EntryModel : PageModel
    {

        private HttpClient HttpClient { get; }
        private IConfiguration Configuration { get; }
        private IMemoryCache _cache;

        public EntryModel(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
        {
            HttpClient = httpClient;
            Configuration = configuration;
            _cache = cache;
        }

        public void CacheTryGetValueSet()
        {
            // Look for cache key.
            if (!_cache.TryGetValue(CacheKeys.Entry, out DateTime cacheEntry))
            {
                // Key not in cache, so get data.
                cacheEntry = DateTime.Now;

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromSeconds(3));

                // Save data in cache.
                _cache.Set(CacheKeys.Entry, cacheEntry, cacheEntryOptions);
            }

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

            var currencyList = sb.ToString();



            //var a = 1;


            //if (!_cache.TryGetValue("currencyList", out string currencyList))
            //{

            //    var currencyConverter = new CurrencyConverter(HttpClient, Configuration);
            //    var list = await currencyConverter.GetList();
            //    var sb = new StringBuilder();

            //    foreach (var currency in list)
            //    {
            //        sb.AppendLine(currency.ToString());
            //    }

            //    var cacheEntryOptions = new MemoryCacheEntryOptions()
            //        // Keep in cache for this time, reset time if accessed.
            //        .SetSlidingExpiration(TimeSpan.FromSeconds(100));

            //    currencyList = sb.ToString();

            //    _cache.Set("currencyList", currencyList, cacheEntryOptions);
            //}


            //    if (!_cache.TryGetValue(CacheKeys.Entry, out DateTime cacheEntry))
            //{
            //    // Key not in cache, so get data.
            //    cacheEntry = DateTime.Now;

            //    // Set cache options.
            //    var cacheEntryOptions = new MemoryCacheEntryOptions()
            //        // Keep in cache for this time, reset time if accessed.
            //        .SetSlidingExpiration(TimeSpan.FromSeconds(1000));

            //    // Save data in cache.
            //    _cache.Set(CacheKeys.Entry, cacheEntry, cacheEntryOptions);
            //}

            @ViewData["Data1"] = currencyList;
            //@ViewData["Data2"] = DateTime.Now.ToLongTimeString();
        }
    }
}
