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
            //isnesti i singleton


            await EmailSender.SendFarewellEmailAsync(user.Email);
            //await EntryDatabase.DeleteListAsync(x => x.Id == UserId, EntryType.Income);
            await GoalDatabase.DeleteListAsync(x => x.Id == UserId);
            //ViewData["EntryList"] = user.Email + "Vienas";
            //ViewData["EntryList"] = user.Email + "Du";
            //await EmailSender.SendFarewellEmailAsync(user.Email);
            //await EntryDatabase.DeleteListAsync(x => x.Id == UserId, EntryType.Income);
            //ViewData["EntryList"] = user.Email + "Trys";
            //await EmailSender.SendFarewellEmailAsync(user.Email);
            //await EntryDatabase.DeleteListAsync(x => x.Id == UserId, EntryType.Expense);
            //ViewData["EntryList"] = user.Email + "Keturi";
            //await EmailSender.SendFarewellEmailAsync(user.Email);
            //await GoalDatabase.DeleteListAsync(x => x.Id == UserId);
            //ViewData["EntryList"] = user.Email + "Penki";
            //await EmailSender.SendFarewellEmailAsync(user.Email);
            //Response.Redirect("/index");
        }

        public async Task OnGet()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            
            //var ids = new List<int>{1494, 1497, 1495 , 1496, 1501 };
            //await EntryDatabase.DeleteListAsync(ids ,userId, EntryType.Income);
            //ViewData["EntryList"] = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);
            await UserDatabase.DeleteUserAsync(UserId);
        }
    }
}
