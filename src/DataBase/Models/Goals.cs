using System.ComponentModel.DataAnnotations;
using ePiggyWeb.DataManagement.Goals;

namespace ePiggyWeb.DataBase.Models
{
    public class Goals
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Price { get; set; }
        [StringLength(255)]
        public string Title { get; set; }

        public Goals() { }

        public Goals(Goal goal, int userId)
        {
            UserId = userId;
            Price = goal.Price;
            Title = goal.Title;
        }

        public void Edit(Goal newGoal)
        {
            Price = newGoal.Price;
            Title = newGoal.Title;
        }
    }
}
