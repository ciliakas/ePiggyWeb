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
    public class IncomeGraphModel : PageModel
    {
        private readonly ILogger<IncomeGraphModel> _logger;
        public bool WasException { get; set; }
        public IEntryList Income { get; set; }
        private int UserId { get; set; }
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public string ErrorMessage = "";
        private EntryDatabase EntryDatabase { get; }
        private IConfiguration Configuration { get; }
        public IncomeGraphModel(EntryDatabase entryDatabase, ILogger<IncomeGraphModel> logger, IConfiguration configuration)
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
            TimeManager.SetDate(startDate, endDate, ref ErrorMessage, Response, Request, out var tempStartDate,
                out var tempEndDate);

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
                var entryList = await EntryDatabase.ReadListAsync(x => x.Date >= StartDate && x.Date <= EndDate,
                    UserId,
                    EntryType.Income);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Income = EntryList.RandomList(Configuration, EntryType.Income);
            }
        }
    }
}
