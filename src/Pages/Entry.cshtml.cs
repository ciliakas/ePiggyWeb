using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ePiggyWeb.Pages
{
    public class EntryModel : PageModel
    {
        private EntryDatabase EntryDatabase { get; }
        private GoalDatabase GoalDatabase { get; }
        private UserDatabase UserDatabase { get; }
        private EmailSender EmailSender { get; }
        private IConfiguration Configuration { get; }
        private int UserId { get; set; }
        public EntryModel(EntryDatabase entryDatabase, GoalDatabase goalDatabase,UserDatabase userDatabase, IOptions<EmailSender> emailSenderSettings, IConfiguration configuration)
        {
            EntryDatabase = entryDatabase;
            GoalDatabase = goalDatabase;
            UserDatabase = userDatabase;
            EmailSender = emailSenderSettings.Value;
            Configuration = configuration;
            UserDatabase.Deleted += OnDeleteUser;
        }

        private async void OnDeleteUser(object sender, UserModel user)
        {
            await EmailSender.SendFarewellEmailAsync(user.Email);
        }

        public async Task OnGet()
        {
            //@ViewData["EntryList"] = EntryList.RandomList(Configuration, EntryType.Expense);
            @ViewData["IncomeList"] = EntryList.RandomList(Configuration, EntryType.Income);
            @ViewData["ExpenseList"] = EntryList.RandomList(Configuration, EntryType.Expense);
            @ViewData["GoalList"] = GoalList.RandomList(Configuration);

        }
    }
}
