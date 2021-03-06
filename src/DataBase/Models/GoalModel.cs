﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ePiggyWeb.DataManagement.Goals;

namespace ePiggyWeb.DataBase.Models
{
    public class GoalModel : IGoalModel
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [Column(TypeName = "decimal(18,5)")]
        public decimal Price { get; set; }
        [StringLength(255)]
        public string Title { get; set; }
        [StringLength(3)]
        public string Currency { get; set; }
        public GoalModel() { }

        public GoalModel(IGoal goal, int userId)
        {
            UserId = userId;
            Price = goal.Amount;
            Title = goal.Title;
            Currency = goal.Currency;
        }

        public void Edit(IGoal newGoal)
        {
            Price = newGoal.Amount;
            Title = newGoal.Title;
            Currency = newGoal.Currency;
        }
    }
}
