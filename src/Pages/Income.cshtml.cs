using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class IncomesModel : PageModel
    {
        private readonly ILogger<IncomeModel> _logger;
        public bool WasException { get; set; }
        public IEntryList Income { get; set; }
        public IEnumerable<IEntry> IncomeToDisplay { get; set; }

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

        public decimal AllIncome { get; set; }

        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public string ErrorMessage = "";
        private EntryDatabase EntryDatabase { get; }
        private IConfiguration Configuration { get; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public const int PageSize = 10;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(Income.Count, PageSize));
        public bool ShowPrevious => CurrentPage > 1;
        public bool ShowNext => CurrentPage < TotalPages;

        public IncomesModel(EntryDatabase entryDatabase, ILogger<IncomeModel> logger, IConfiguration configuration)
        {
            EntryDatabase = entryDatabase;
            _logger = logger;
            Configuration = configuration;
        }

        public async Task OnGet()
        {
            TimeManager.GetDate(Request, out var tempStartDate, out var tempEndDate);
            StartDate = tempStartDate;
            EndDate = tempEndDate;

            await SetData();
        }

        public async Task<IActionResult> OnGetFilter(DateTime startDate, DateTime endDate)
        {
            TimeManager.SetDate(startDate, endDate, ref ErrorMessage, Response, Request,out var tempStartDate, out var tempEndDate);
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
                await EntryDatabase.CreateAsync(entry, UserId, EntryType.Income);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }
            
            return RedirectToPage("/income");
        }

        public async Task<IActionResult> OnPostDelete()
        {
            try
            {
                var selected = Request.Form["chkEntry"].ToString();
                var selectedList = selected.Split(',');
                var entryIdList = selectedList.Select(temp => Convert.ToInt32(temp)).ToList();

                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);

                await EntryDatabase.DeleteListAsync(entryIdList, UserId, EntryType.Income);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;

            }
            return RedirectToPage("/income");
        }

        private async Task SetData()
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var entryList = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);
                Income = entryList.GetFrom(StartDate).GetTo(EndDate);
                IncomeToDisplay = Income.OrderByDescending(x => x.Date).Skip((CurrentPage - 1) * PageSize).Take(PageSize);
                AllIncome = entryList.GetSum();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Income = EntryList.RandomList(Configuration, EntryType.Income);
                IncomeToDisplay = Income;
                AllIncome = Income.GetSum();
            }

        }
    }
}
