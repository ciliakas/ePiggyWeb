using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
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
    public class ExpensesGraphModel : PageModel
    {
        private readonly ILogger<ExpensesGraphModel> _logger;
        public bool WasException { get; set; }
        public IEntryList Expenses { get; set; }
        private int UserId { get; set; }
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public string ErrorMessage = "";
        private EntryDatabase EntryDatabase { get; }
        private IConfiguration Configuration { get; }

        public ExpensesGraphModel(EntryDatabase entryDatabase, ILogger<ExpensesGraphModel> logger, IConfiguration configuration)
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
            TimeManager.SetDate(startDate, endDate, ref ErrorMessage, Response, Request, out var tempStartDate, out var tempEndDate);
            StartDate = tempStartDate;
            EndDate = tempEndDate;
            await SetData();
            return Page();
        }

        private async Task SetData()
        {
            //throw new Exception();
            try
            {
                //throw new Exception();
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var entryList = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
                Expenses = entryList.GetFrom(StartDate).GetTo(EndDate);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Expenses = EntryList.RandomList(Configuration, EntryType.Expense);
            }

        }
    }
}
