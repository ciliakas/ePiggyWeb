using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class ComparisonGraphModel : PageModel
    {
        private readonly ILogger<ComparisonGraphModel> _logger;
        public bool WasException { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        private int UserId { get; set; }
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public string ErrorMessage = "";
        private EntryDatabase EntryDatabase { get; }
        public ComparisonGraphModel(EntryDatabase entryDatabase, ILogger<ComparisonGraphModel> logger)
        {
            EntryDatabase = entryDatabase;
            _logger = logger;
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
            TimeManager.SetDate(startDate, endDate, ref ErrorMessage, Response, Request, out var tempStartDate, out var tempEndDate);
            StartDate = tempStartDate;
            EndDate = tempEndDate;
            await SetData();
            return Page();
        }

        private async Task SetData()
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                Expenses = (await EntryDatabase.ReadListAsync(UserId, EntryType.Expense)).GetFrom(StartDate)
                    .GetTo(EndDate).GetSum();
                Income = (await EntryDatabase.ReadListAsync(UserId, EntryType.Income)).GetFrom(StartDate).GetTo(EndDate)
                    .GetSum();
            }
            catch (Exception ex)
            {
                var rand = new Random();
                const int min = 10000;
                const int max = 500001;
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Expenses = (decimal)rand.Next(min,max)/100;
                Income = (decimal)rand.Next(min, max) / 100;
            }

        }
    }
}
