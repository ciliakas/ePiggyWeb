using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class EntryModel : PageModel
    {
        public void OnGet()
        {

            var dataManager = new DataManager();
            ViewData["EntryList"] = dataManager.Income.ToString();
        }
    }
}
