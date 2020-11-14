using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class IncomesModel : PageModel
    {
        public IEntryList Income { get; set; }

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

        public decimal AllIncome { get; set; }

        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public string ErrorMessage = "";
        private EntryDb EntryDb { get; }
        public IncomesModel(EntryDb entryDb)
        {
            EntryDb = entryDb;
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
            await EntryDb.CreateAsync(entry, UserId, EntryType.Income);
            return RedirectToPage("/income");
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            await EntryDb.DeleteAsync(id, UserId, EntryType.Income);
            return RedirectToPage("/income");
        }

        private async Task SetData()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var entryList = await EntryDb.ReadListAsync(UserId, EntryType.Income);
            Income = entryList.GetFrom(StartDate).GetTo(EndDate);
            AllIncome = entryList.GetSum();
        }
    }
}
