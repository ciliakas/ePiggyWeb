using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class GoalsModel : PageModel
    {
        private readonly ILogger<GoalsModel> _logger;
        public bool WasException { get; set; }
        public Lazy<InternetParser> InternetParser;
        public IGoalList Goals { get; set; }
        public decimal Savings { get; set; }
        private int UserId { get; set; }

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        [StringLength(30)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        [Range(0, 99999999.99)]
        public decimal Amount { get; set; }

        private GoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }
        private HttpClient HttpClient { get; }
        private IConfiguration Configuration { get; }

        public GoalsModel(GoalDatabase goalDatabase, EntryDatabase entryDatabase, ILogger<GoalsModel> logger, HttpClient httpClient, IConfiguration configuration)
        {
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            _logger = logger;
            HttpClient = httpClient;
            InternetParser = new Lazy<InternetParser>(() => new InternetParser(HttpClient));
            Configuration = configuration;
        }

        public async Task OnGet()
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                Goals = await GoalDatabase.ReadListAsync(UserId);

                var income = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);
                var expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
                Savings = income.GetSum() - expenses.GetSum();
                if (Savings < 0)
                {
                    Savings = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Goals = GoalList.RandomList(Configuration);
                Savings = 0;
            }
        }

        public async Task<IActionResult> OnPostNewGoal()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await OnGet();
                    return Page();
                }

                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var temp = Goal.CreateLocalGoal(Title, Amount);
                await GoalDatabase.CreateAsync(temp, UserId);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }
           
            return RedirectToPage("/goals");
        }

        public async Task<IActionResult> OnPostParseGoal()
        {
            try
            {
                if (string.IsNullOrEmpty(Title))
                {
                    await OnGet();
                    return Page();
                }

                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var temp = await InternetParser.Value.ReadPriceFromCamel(Title);
                await GoalDatabase.CreateAsync(temp, UserId);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }
            return RedirectToPage("/goals");

        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                await GoalDatabase.DeleteAsync(id, UserId);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }
            return RedirectToPage("/goals");
        }

        public async Task<IActionResult> OnPostPurchased(int id, string title, string amount)
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                decimal.TryParse(amount, out var parsedAmount);
                var entry = Entry.CreateLocalEntry(title, parsedAmount, DateTime.Today, recurring: false,
                    importance: 1);
                await GoalDatabase.MoveGoalToExpensesAsync(id, UserId, entry);
                return RedirectToPage("/expenses");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                return RedirectToPage("/goals");
            }
            
        }

    }
}
