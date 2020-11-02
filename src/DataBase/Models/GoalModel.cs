using System.ComponentModel.DataAnnotations;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Goals;

namespace ePiggyWeb.DataBase.Models
{
    public class GoalModel
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        [StringLength(255)]
        public string Title { get; set; }

        public GoalModel() { }

        public GoalModel(IFinanceable goal, int userId)
        {
            UserId = userId;
            Amount = goal.Amount;
            Title = goal.Title;
        }

        public void Edit(Goal newGoal)
        {
            Amount = newGoal.Amount;
            Title = newGoal.Title;
        }
    }
}
