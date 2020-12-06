using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.MonthlyReport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    public class MonthlyReportModel : PageModel
    {
        private readonly ILogger<SavingSuggestionsModel> _logger;
        public bool WasException { get; set; }
        private int UserId { get; set; }

        public string ErrorMessage = "";

        [BindProperty]
        public MonthlyReportResult Data { get; set; }

        private GoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }

        public MonthlyReportModel(ILogger<SavingSuggestionsModel> logger, GoalDatabase goalDatabase, EntryDatabase entryDatabase)
        {
            _logger = logger;
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
        }
        public async Task<IActionResult> OnGet()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var monthlyReportCalculator = new MonthlyReportCalculator(GoalDatabase, EntryDatabase, UserId);
            Data = await monthlyReportCalculator.Calculate();
            return Page();
        }

    }
}
