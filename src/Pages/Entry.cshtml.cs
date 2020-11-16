using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
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
        public EntryModel(EntryDatabase entryDatabase)
        {
            EntryDatabase = entryDatabase;
        }

        public async Task OnGet()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            //var ids = new List<int>{1494, 1497, 1495 , 1496, 1501 };
            //await EntryDatabase.DeleteListAsync(ids ,userId, EntryType.Income);
            ViewData["EntryList"] = await EntryDatabase.ReadListAsync(userId, EntryType.Income);
        }
    }
}
