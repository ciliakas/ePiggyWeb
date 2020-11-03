using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class EntryModel : PageModel
    {
        public void OnGet()
        {
            //var entry1 = new Entry("entry1", 100M,  DateTime.Today, false, 2);
            //var entry2 = new Entry("entry2", 200M,  DateTime.Today, true, 2);
            //var goal1 = new Goal("goal1", 150M);
            //var goal2 = new Goal("goal2", 100M);
            //var goal3 = new Goal("goal3", 200M);
            //var goal4 = new Goal("goal4", 250M);
            //var goal5 = new Goal("goal5", 100M);
            //var decimal1 = 100M;
            //var decimal2 = 150M;
            //ViewData["EntryList"] = goal2.Equals(decimal1);
            //var dataManager = new DataManager();
            //ViewData["EntryList"] = dataManager.Income.ToString();

            Entry entry1 = new Entry();
            IEntry entry2 = new Entry();
            IGoal entry3 = new Entry();

            entry2.Title = "labas";
            entry1.Edit(entry2);

            Goal goal1 = new Goal();
            IGoal goal2 = new Goal();
            //IEntry goal3 = new Goal(); -- thats illegla

            ViewData["EntryList"] = entry1.ToString();
        }
    }
}
