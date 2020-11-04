using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class EditEntryModel : PageModel
    {
        public Entry Entry { get; set; }
        public int EntryType { get; set; }


        [Required(ErrorMessage = "Title is required")]
        [BindProperty]
        public string Title { get; set; }
        [Required(ErrorMessage = "Amount is required")]
        [BindProperty]
        public string Amount { get; set; }
        [BindProperty]
        public string Date { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Importance is required")]
        public string Importance { get; set; }
        [BindProperty]
        public string IsMonthly { get; set; }

        public string Error { get; set; }
        public void OnGet(int id, int entryType)
        {
            EntryType = entryType;
            //var dataManager = new DataManager();
            if (entryType == 1)//if 1 income if 2 expense
            {
                //Code to get entry by ID
               // Entry = dataManager.Income.
            }

        }

        public void OnPost()
        {
            //code to edit entry
        }


    }
}
