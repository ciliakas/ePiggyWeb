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
using ePiggyWeb.DataManagement.Entries;
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

        public async Task<Tuple<IList<Currency>, Exception>> GetCurrencyList(int userId)
        {
            if (!Cache.TryGetValue(CacheKeys.CurrencyList, out IList<Currency> currencyList))
            {
                try
                {
                    currencyList = await CurrencyApiAgent.GetList();
                }
                catch (Exception apiException)
                {
                    try
                    {
                        var userModel = await UserDatabase.GetUserAsync(userId);
                        var userCurrency = new Currency { Code = userModel.Currency };
                        return new Tuple<IList<Currency>, Exception>(new List<Currency> { userCurrency }, apiException);
                    }
                    catch (Exception dbException)
                    {
                        return new Tuple<IList<Currency>, Exception>(new List<Currency>(), dbException);
                    }
                }
            }

            return new Tuple<IList<Currency>, Exception>(currencyList, null);
        }

        public async Task<IEntryList> ConvertEntryList(IEntryList entryList, int userId)
        {
            var (userCurrency, exception) = await GetUserCurrency(userId);
            //Error check

            //Temporary while no currency is saved
            foreach (var entry in entryList)
            {
                if (entry.Currency is null || entry.Currency == "currency") entry.Currency = "EUR";
            }

            var containsForeignCurrency = entryList.Any(x => x.Currency != userCurrency.Code);
            if (!containsForeignCurrency) return entryList;

            var currencyList = await CurrencyApiAgent.GetList();
            //Error check
            foreach (var entry in entryList.Where(x => x.Currency != userCurrency.Code))
            {
                var userRate = userCurrency.Rate;
                var foreignRate = currencyList.First(x => x.Code == entry.Currency).Rate;
                var rate = userRate / foreignRate;
                entry.Amount *= rate;
            }

            return entryList;
        }

        public async Task<Tuple<Currency, Exception>> GetUserCurrency(int userId)
        {
            if (!Cache.TryGetValue(CacheKeys.UserCurrency, out Currency userCurrency))
            {
                UserModel userModel;
                try
                {
                    userModel = await UserDatabase.GetUserAsync(userId);
                }
                catch (Exception ex)
                {
                    return new Tuple<Currency, Exception>(new Currency { Code = "EUR", Rate = 1, SymbolString = "EUR" }, ex);
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
