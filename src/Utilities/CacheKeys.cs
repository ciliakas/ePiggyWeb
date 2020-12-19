using System;
using Microsoft.Extensions.Caching.Memory;

namespace ePiggyWeb.Utilities
{
    public static class CacheKeys
    {
        public static string CurrencyList => "_CurrencyList";
        public static string UserCurrency => "_UserCurrency";

        public static MemoryCacheEntryOptions DefaultCurrencyCacheOptions()
        {
            var options = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15))
                .SetAbsoluteExpiration(TimeManager.RefreshTime());

            return options;
        }
    }
}
