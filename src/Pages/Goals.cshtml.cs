using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class GoalsModel : PageModel
    {
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
        public GoalsModel(GoalDatabase goalDatabase, EntryDatabase entryDatabase, HttpClient httpClient)
        {
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            InternetParser = new Lazy<InternetParser>(() => new InternetParser(httpClient));
        }

        public async Task OnGet()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            Goals = await GoalDatabase.ReadListAsync(UserId);
            var incomes = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);
            var expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
            Savings = incomes.GetSum() - expenses.GetSum();
            if (Savings < 0)
            {
                Savings = 0;
            }
        }

        public async Task<IActionResult> OnPostNewGoal()
        { 
            if (!ModelState.IsValid)
            {
                await OnGet();
                return Page();
            }
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var temp = Goal.CreateLocalGoal(Title, Amount);
            await GoalDatabase.CreateAsync(temp, UserId);
            return RedirectToPage("/goals");
        }

        public async Task<IActionResult> OnPostParseGoal()
        {
            if (string.IsNullOrEmpty(Title))
            {
                await OnGet();
                return Page();
            }
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var temp = await InternetParser.Value.ReadPriceFromCamel(Title);
            await GoalDatabase.CreateAsync(temp, UserId);
            return RedirectToPage("/goals");
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            await DeleteGoalFromDb(id);
            return RedirectToPage("/goals");
        }

        public async Task<IActionResult> OnPostPurchased(int id, string title, string amount)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            decimal.TryParse(amount, out var parsedAmount);
            var entry = Entry.CreateLocalEntry(title, parsedAmount, DateTime.Today, recurring:false, importance:1);
            await GoalDatabase.MoveGoalToExpensesAsync(id, UserId, entry);
            return RedirectToPage("/expenses");
        }

        private async Task DeleteGoalFromDb(int id)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            await GoalDatabase.DeleteAsync(id, UserId);
        }
    }
}
