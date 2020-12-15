using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Authorization;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class ExpensesModel : PageModel
    {
        private readonly ILogger<ExpensesModel> _logger;
        public bool WasException { get; set; }
        public IEntryList Expenses { get; set; }

        [Required(ErrorMessage = "Title Required.")]
        [BindProperty]
        [StringLength(25, ErrorMessage = "Too long title!")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Amount Required.")]
        [BindProperty]
        [Range(0, 99999999.99, ErrorMessage = "Amount out of range!")]
        public decimal Amount { get; set; }
        [BindProperty]
        public DateTime Date { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Importance Required.")]
        public int Importance { get; set; }
        [BindProperty]
        public bool Recurring { get; set; }

        private int UserId { get; set; }

        public decimal AllExpenses { get; set; }

        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public string ErrorMessage = "";

        private EntryDatabase EntryDatabase { get; }
        private IConfiguration Configuration { get; }
        public UserDatabase UserDatabase { get; }
        public CurrencyConverter CurrencyConverter { get; }
        public string CurrencySymbol { get; private set; }
        public ExpensesModel(EntryDatabase entryDatabase, ILogger<ExpensesModel> logger, IConfiguration configuration, UserDatabase userDatabase, CurrencyConverter currencyConverter)
        {
            EntryDatabase = entryDatabase;
            _logger = logger;
            Configuration = configuration;
            UserDatabase = userDatabase;
            CurrencyConverter = currencyConverter;
        }

        public async Task OnGet()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var userModel = await UserDatabase.GetUserAsync(UserId);
            CurrencySymbol = await CurrencyConverter.GetCurrencySymbol(userModel.Currency);
            TimeManager.GetDate(Request, out var tempStartDate, out var tempEndDate);
            StartDate = tempStartDate;
            EndDate = tempEndDate;
            await SetData();
        }

        public async Task<IActionResult> OnGetFilter(DateTime startDate, DateTime endDate)
        {
            TimeManager.SetDate(startDate, endDate, ref ErrorMessage, Response, Request, out var tempStartDate, out var tempEndDate);
            StartDate = tempStartDate;
            EndDate = tempEndDate;
            await SetData();
            return Page();
        }

        public async Task<IActionResult> OnPostNewEntry()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var today = DateTime.Now;
                    StartDate = new DateTime(today.Year, today.Month, 1);
                    EndDate = DateTime.Today;
                    await SetData();
                    return Page();
                }

                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var entry = Entry.CreateLocalEntry(Title, Amount, Date, Recurring, Importance);
                await EntryDatabase.CreateAsync(entry, UserId, EntryType.Expense);
                return RedirectToPage("/expenses");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                return Page();
            }
           
            
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            try
            {
                var selected = Request.Form["chkEntry"].ToString();
                var selectedList = selected.Split(',');
                var entryIdList = selectedList.Select(temp => Convert.ToInt32(temp)).ToList();

                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);

                await EntryDatabase.DeleteListAsync(entryIdList, UserId, EntryType.Expense);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }
           

                /* try
                 {
                     UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                     await EntryDatabase.DeleteAsync(id, UserId, EntryType.Expense);
                 }
                 catch (Exception ex)
                 {
                     _logger.LogInformation(ex.ToString());
                     WasException = true;
                 }*/

                return RedirectToPage("/expenses");
        }

        private async Task SetData()
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var entryList = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
                Expenses = entryList.GetFrom(StartDate).GetTo(EndDate);
                AllExpenses = entryList.GetSum();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Expenses = EntryList.RandomList(Configuration, EntryType.Expense);
                AllExpenses = Expenses.GetSum();
            }
        }
    }
}
