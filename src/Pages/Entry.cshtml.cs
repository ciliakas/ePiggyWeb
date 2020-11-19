using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class EntryModel : PageModel
    {
        private EntryDatabase EntryDatabase { get; }
        private GoalDatabase GoalDatabase { get; }
        private UserDatabase UserDatabase { get; }
        private EmailSender EmailSender { get; }
        private int UserId { get; set; }
        public EntryModel(EntryDatabase entryDatabase, GoalDatabase goalDatabase,UserDatabase userDatabase, EmailSender emailSender)
        {
            EntryDatabase = entryDatabase;
            GoalDatabase = goalDatabase;
            UserDatabase = userDatabase;
            EmailSender = emailSender;
            UserDatabase.Deleted += OnDeleteUser;
        }

        private async void OnDeleteUser(object sender, UserModel user)
        {
            await EmailSender.SendFarewellEmailAsync(user.Email);
        }

        public async Task OnGet()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            await UserDatabase.DeleteUserAsync(UserId);
        }
    }
}
