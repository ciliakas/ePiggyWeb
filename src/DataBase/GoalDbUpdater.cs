using System;
using System.Linq;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataBase
{
    public static class GoalDbUpdater
    {
        public static int Add(Goal goal, int userid)
        {
            var db = new DatabaseContext();
            var dbGoal = new GoalModel (goal, userid);
            db.Add(dbGoal);
            db.SaveChanges();
            return dbGoal.Id;
        }

        public static bool Remove(Goal goal)
        {
            var db = new DatabaseContext();
            try
            {
                var dbGoal = db.Goals.FirstOrDefault(x => x.Id == goal.Id);
                db.Goals.Remove(dbGoal ?? throw new InvalidOperationException());
                db.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                ExceptionHandler.Log("Couldn't find goal id: " + goal.Id + " in database");
                return false;
            }
            return true;
        }

        public static bool Edit(Goal oldGoal, Goal newGoal)
        {
            var db = new DatabaseContext();
            var dbGoal = db.Goals.FirstOrDefault(x => x.Id == oldGoal.Id);
            if (dbGoal == null)
            {
                ExceptionHandler.Log("Couldn't find goal id: " + oldGoal.Id + " in database");
                return false;
            }
            dbGoal.Edit(newGoal);
            db.SaveChanges();
            return true;
        }
    }
}
