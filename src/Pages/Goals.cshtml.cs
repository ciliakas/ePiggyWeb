using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Goals;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class GoalsModel : PageModel
    {
        public IGoalList Goals { get; set; }
        public void OnGet()
        {
            var dataManager = new DataManager();
            Goals = dataManager.Goals.GoalList;
        }


    }
}
