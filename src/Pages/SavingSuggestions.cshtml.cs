using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.DataManagement.Saving;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class SavingSuggestionsModel : PageModel
    {
        public bool ExceptionHappened { get; set; }
        public IGoal Goal { get; set; }
        public decimal Savings { get; set; }
        private int UserId { get; set; }
        public IEntryList Expenses { get; set; }
        public int MonthsToSave { get; set; }
        [BindProperty]
        public int Id { get; set; }

       // public IList<ISavingSuggestion> EntrySuggestions { get; set; }
       // public List<SavingSuggestionByImportance> MonthlySuggestions { get; set; }
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }
        public CalculationResults MinimalSuggestions { get; set; }
        public CalculationResults RegularSuggestions { get; set; }
        public CalculationResults MaximalSuggestions { get; set; }

       // private readonly AlternativeSavingCalculator alternativeSavingCalculator = new AlternativeSavingCalculator();
        private readonly ThreadingCalculator _threadingCalculator = new ThreadingCalculator();

        public string ErrorMessage = "";
        public void OnGet(int id)
        {
            Id = id;
            var today = DateTime.Now;
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = DateTime.Today;
            SetData();
            
        }

        public IActionResult OnGetFilter(DateTime startDate, DateTime endDate, int id)
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

            Id = id;
            SetData();
            return Page();
        }

        public void SetData()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var dataManager = new DataManager(UserId);
            Expenses = dataManager.Expenses.EntryList.GetFrom(StartDate).GetTo(EndDate);
            Goal = dataManager.Goals.GoalList.FirstOrDefault(x => x.Id == Id);
            Savings = dataManager.Income.EntryList.GetSum() - dataManager.Expenses.EntryList.GetSum();
            if (Savings < 0)
            {
                Savings = 0;
            }

            try
            {                
                //var calculationResults = alternativeSavingCalculator.GetSuggestedExpensesOffers(Expenses, Goal, Savings);
                //EntrySuggestions = calculationResults.EntrySuggestions;
                //MonthlySuggestions = calculationResults.ImportanceSuggestions;
                //MonthsToSave = calculationResults.TimesToRepeatSaving;

                var suggestionDictionary = _threadingCalculator.GetAllSuggestedExpenses(Expenses, Goal, Savings);
                MinimalSuggestions = suggestionDictionary[SavingType.Minimal];
                RegularSuggestions = suggestionDictionary[SavingType.Regular];
                MaximalSuggestions = suggestionDictionary[SavingType.Maximal];
            }
            catch(Exception ex)
            {
                ExceptionHandler.Log(ex.ToString());
                ExceptionHappened = true;
            }
        }
    }
}
