using System;
using System.Collections.Generic;
using System.Linq;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataBase
{
    public static class DatabaseUpdater
    {
        public static int AddGoal(int userid, string title, decimal value)
        {
            var db = new DatabaseContext();
            var goal = new Goals { UserId = userid, Title = title, Price = value };
            db.Add(goal);
            db.SaveChanges();
            return goal.Id;
        }

        public static int AddIncome(int userid, decimal value, string title, DateTime date, bool isMonthly,
            int importance)
        {
            var db = new DatabaseContext();
            var income = new Incomes { UserId = userid, Amount = value, Date = date, IsMonthly = isMonthly, Title = title, Importance = importance };
            db.Add(income);
            db.SaveChanges();
            return income.Id;
        }

        public static int AddExpense(int userid, decimal value, string title, DateTime date, bool isMonthly,
            int importance)
        {
            var db = new DatabaseContext();
            var expense = new Expenses { UserId = userid, Amount = value, Date = date, IsMonthly = isMonthly, Title = title, Importance = importance };
            db.Add(expense);
            db.SaveChanges();
            return expense.Id;
        }

        public static void RemoveGoal(int id)
        {
            var db = new DatabaseContext();
            try
            {
                var index = db.Goals.FirstOrDefault(x => x.Id == id);
                db.Goals.Remove(index ?? throw new InvalidOperationException());
                db.SaveChanges();
			}
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
            }
        }
        
        public static void RemoveIncome(int id)
        {
            var db = new DatabaseContext();
            try
            {
                var index = db.Incomes.FirstOrDefault(x => x.Id == id);
                db.Incomes.Remove(index ?? throw new InvalidOperationException());
                db.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
				ExceptionHandler.Log(ex.ToString());
            }
        }

        public static void RemoveIncomes(IEnumerable<DataEntry> entries)
        {
            var db = new DatabaseContext();
            var list = new List<Incomes>();

            foreach (var z in entries)
            {
                var index = db.Incomes.FirstOrDefault(x => x.Id == z.Id);
                list.Add(index);
            }
            db.Incomes.RemoveRange(list ?? throw new InvalidOperationException());
            db.SaveChanges();
        }

        public static void RemoveIncome(DataEntry dataEntry)
        {
            var db = new DatabaseContext();
            try
            {
                var index = db.Incomes.FirstOrDefault(x => x.Id == dataEntry.Id);
                db.Incomes.Remove(index ?? throw new InvalidOperationException());
                db.SaveChanges();
            }
            catch(InvalidOperationException ex)
            {
				ExceptionHandler.Log(ex.ToString());
            }
        }

        public static void RemoveExpenses(IEnumerable<DataEntry> entries)
        {
            var db = new DatabaseContext();
            var list = new List<Expenses>();

            foreach (var z in entries)
            {
                var index = db.Expenses.FirstOrDefault(x => x.Id == z.Id);
                list.Add(index);
            }
            db.Expenses.RemoveRange(list);
            db.SaveChanges();
        }

        public static void RemoveExpense(DataEntry dataEntry)
        {
            var db = new DatabaseContext();
            try
            {
                var index = db.Expenses.FirstOrDefault(x => x.Id == dataEntry.Id);
                db.Expenses.Remove(index ?? throw new InvalidOperationException());
                db.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
            }
        }

        public static void RemoveExpense(int id)
        {
            var db = new DatabaseContext();
            try
            {
                var index = db.Expenses.FirstOrDefault(x => x.Id == id);
                db.Expenses.Remove(index ?? throw new InvalidOperationException());
                db.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
            }
        }
        
        public static bool EditGoal(int id, string title, decimal value)
        {
            var db = new DatabaseContext();
            var temp = db.Goals.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Title = title;
            temp.Price = value;
            db.SaveChanges();
            return true;
        }

        /*public static bool EditGoalPlaceInQueue(int id, int placeInQueue)
        {
            var db = new DatabaseContext();
            var temp = db.Goals.FirstOrDefault(x => x.Id == id);
            if (temp != null)
            {
                temp.PlaceInQueue = placeInQueue;
                db.SaveChanges();
                return true;
            }
            return false;
        }*/

        public static bool EditIncomeItem(int id, decimal value)
        {
            var db = new DatabaseContext();
            var temp = db.Incomes.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Amount = value;
            db.SaveChanges();
            return true;
        }
        
        public static bool EditIncomeItem(int id, string value)
		{
			var db = new DatabaseContext();
			var temp = db.Incomes.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Title = value;
            db.SaveChanges();
            return true;
        }
        
        public static bool EditIncomeItem(int id, DateTime date)
		{
			var db = new DatabaseContext();
			var temp = db.Incomes.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Date = date;
            db.SaveChanges();
            return true;
        }
        
		public static bool EditIncomeItem(int id, bool isMonthly)
		{
			var db = new DatabaseContext();
			var temp = db.Incomes.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.IsMonthly = isMonthly;
            db.SaveChanges();
            return true;
        }
        
        public static bool EditIncomeItem(int id, int importance)
        {
            var db = new DatabaseContext();
            var temp = db.Incomes.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Importance = importance;
            db.SaveChanges();
            return true;
        }
        
        public static bool EditIncomeItem(int id, string value, decimal amount, DateTime date, bool isMonthly, int importance)
		{
			var db = new DatabaseContext();
			var temp = db.Incomes.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Title = value;
            temp.Amount = amount;
            temp.Date = date;
            temp.IsMonthly = isMonthly;
            temp.Importance = importance;
            db.SaveChanges();
            return true;
        }
        
        public static bool EditExpensesItem(int id, decimal value)
		{
			var db = new DatabaseContext();
			var temp = db.Expenses.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Amount = value;
            db.SaveChanges();
            return true;
        }
        
        public static bool EditExpensesItem(int id, string value)
		{
			var db = new DatabaseContext();
			var temp = db.Expenses.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Title = value;
            db.SaveChanges();
            return true;

        }
        
        public static bool EditExpensesItem(int id, DateTime date)
		{
			var db = new DatabaseContext();
			var temp = db.Expenses.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Date = date;
            db.SaveChanges();
            return true;
        }
        
        public static bool EditExpensesItem(int id, bool isMonthly)
		{
            var db = new DatabaseContext();
			var temp = db.Expenses.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.IsMonthly = isMonthly;
            db.SaveChanges();
            return true;
        }
        
        public static bool EditExpensesItem(int id, int importance)
		{
			var db = new DatabaseContext();
			var temp = db.Expenses.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Importance = importance;
            db.SaveChanges();
            return true;
        }
        
        public static bool EditExpensesItem(int id, string value, decimal amount, DateTime date, bool isMonthly, int importance)
		{
			var db = new DatabaseContext();
			var temp = db.Expenses.FirstOrDefault(x => x.Id == id);
            if (temp == null)
            {
                return false;
            }
            temp.Title = value;
            temp.Amount = amount;
            temp.Date = date;
            temp.IsMonthly = isMonthly;
            temp.Importance = importance;
            db.SaveChanges();
            return true;
        }
    }
}
