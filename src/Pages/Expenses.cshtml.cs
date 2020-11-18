using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Authorization;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class ExpensesModel : PageModel
    {
        private readonly ILogger<ExpensesModel> _logger;
        public bool WasException { get; set; }
        public IEntryList Expenses { get; set; }

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        [StringLength(30)]
        public string Title { get; set; }
        [Required(ErrorMessage = "Required")]
        [BindProperty]
        [Range(0, 99999999.99)]
        public decimal Amount { get; set; }
        [BindProperty]
        public DateTime Date { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Required")]
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
        public ExpensesModel(EntryDatabase entryDatabase, ILogger<ExpensesModel> logger)
        {
            EntryDatabase = entryDatabase;
            _logger = logger;
        }

        public async Task OnGet()
        {
            var today = DateTime.Now;
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = DateTime.Today;
            await SetData();
        }

        public async Task<IActionResult> OnGetFilter(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                ErrorMessage = "Start date is bigger than end date!";
                var today = DateTime.Now;
                StartDate = new DateTime(today.Year, today.Month, 1);
                EndDate = DateTime.Today;
            }
            else
            {
                StartDate = startDate;
                EndDate = endDate;
            }
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
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }
           
            return RedirectToPage("/expenses");
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                await EntryDatabase.DeleteAsync(id, UserId, EntryType.Expense);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }
            
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
                //custom filling
            }
        }
    }
}
