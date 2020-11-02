using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Goals;

namespace ePiggyWeb.DataBase.Models
{
    public interface IGoalModel
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        [StringLength(255)]
        public string Title { get; set; }

        public void Edit(Goal newGoal)
        {
            Amount = newGoal.Amount;
            Title = newGoal.Title;
        }
    }
}
