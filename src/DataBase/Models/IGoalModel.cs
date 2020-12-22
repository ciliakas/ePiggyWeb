using System.ComponentModel.DataAnnotations;
using ePiggyWeb.DataManagement.Goals;

namespace ePiggyWeb.DataBase.Models
{
    public interface IGoalModel
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Price { get; set; }
        [StringLength(255)]
        public string Title { get; set; }
        [StringLength(3)]
        public string Currency { get; set; }

        public void Edit(IGoal newGoal);
    }
}
