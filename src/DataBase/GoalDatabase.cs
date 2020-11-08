using System;
using System.Collections.Generic;
using System.Linq;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataBase
{
    public static class GoalDatabase
    {
        public static int Create(IGoal goal, int userid)
        {
            var db = new DatabaseContext();
            var dbGoal = new GoalModel (goal, userid);
            db.Add(dbGoal);
            db.SaveChanges();
            return dbGoal.Id;
        }

        public static bool Delete(IGoal goal)
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

        public static bool Delete(int id, int userId)
        {
            var db = new DatabaseContext();
            try
            {
                var dbGoal = db.Goals.FirstOrDefault(x => x.Id == id && x.UserId == userId);
                db.Goals.Remove(dbGoal ?? throw new InvalidOperationException());
                db.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                ExceptionHandler.Log("Couldn't find goal id: " + id + " in database");
                return false;
            }
            return true;
        }

        public static bool Update(IGoal oldGoal, IGoal newGoal)
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

        public static bool Update(int id, int userId, IGoal newGoal)
        {
            var db = new DatabaseContext();
            var dbGoal = db.Goals.FirstOrDefault(x => x.Id == id && x.UserId == userId);
            if (dbGoal == null)
            {
                ExceptionHandler.Log("Couldn't find goal id: " + id + " in database");
                return false;
            }
            dbGoal.Edit(newGoal);
            db.SaveChanges();
            return true;
        }

        public static IGoal Read(int id, int userId)
        {
            using var db = new DatabaseContext();
            var dbGoal = db.Goals.FirstOrDefault(x => x.Id == id && x.UserId == userId);
            return new Goal(dbGoal);
        }

        public static IEnumerable<IGoal> Read(int userId)
        {
            return Read(x => x.UserId == userId);
        }

        public static IEnumerable<IGoal> Read(Func<GoalModel, bool> filter)
        {
            using var db = new DatabaseContext();
            return db.Goals.Where(filter).Select(dbGoal => new Goal(dbGoal)).Cast<IGoal>().ToList();
        }
    }
}
