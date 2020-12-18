using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace ePiggyWeb.CurrencyAPI
{
    public class CurrencyConverter
    {
        private CurrencyApiAgent CurrencyApiAgent { get; }
        private IMemoryCache Cache { get; }
        private UserDatabase UserDatabase { get; }
        public CurrencyConverter(CurrencyApiAgent currencyApiAgent, IMemoryCache cache, UserDatabase userDatabase)
        {
            CurrencyApiAgent = currencyApiAgent;
            Cache = cache;
            UserDatabase = userDatabase;
        }

        private async Task<Tuple<Currency, Exception>> Test()
        {
            throw new NotImplementedException();
        }

        public async Task<Tuple<Currency, Exception>> GetUserCurrency(int userId)
        {
            // We do the whole try thing here, and we throw exceptions down for front to catch and deal with

            if (!Cache.TryGetValue(CacheKeys.UserCurrency, out Currency userCurrency))
            {
                UserModel userModel;
                try
                {
                    userModel = await UserDatabase.GetUserAsync(userId);
                }
                catch (Exception ex)
                {
                    return new Tuple<Currency, Exception>(new Currency{Code = "EUR", Rate = 1, SymbolString = "EUR"}, ex);
                }

                try
                {
                    userCurrency = await CurrencyApiAgent.GetCurrency(userModel.Currency);
                }
                catch (Exception ex)
                {
                    return new Tuple<Currency, Exception>
                        (new Currency { Code = userModel.Currency, Rate = 1, SymbolString = userModel.Currency }, ex);
                }
            }

            var options = CacheKeys.DefaultCurrencyCacheOptions();
            Cache.Set(CacheKeys.UserCurrency, userCurrency, options);
            return new Tuple<Currency, Exception>(userCurrency, null);
        }

    }
}
