using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ePiggyWeb.DataBase.Models;

namespace ePiggyWeb.Utilities
{
    public static class PredicateBuilder
    {
        public static Expression<Func<IEntryModel, bool>> BuildEntryFilter(IEnumerable<int> idEnumerable, int userId)
        {
            if (idEnumerable is null)
            {
                return x => x.UserId == userId;
            }

            var idArray = idEnumerable as int[] ?? idEnumerable.ToArray();

            if (!idArray.Any())
            {
                return x => x.UserId == userId;
            }

            Expression<Func<IEntryModel, bool>> filter = x => x.Id == idArray[0] && x.UserId == userId;

            for (var i = 1; i <= idArray.Length; i++)
            {
                var i1 = i;
                filter = filter.Or(x => x.Id == idArray[i1] && x.UserId == userId);
            }

            return filter;
        }

        public static Expression<Func<IGoalModel, bool>> BuildGoalFilter(IEnumerable<int> idEnumerable, int userId)
        {
            if (idEnumerable is null)
            {
                return x => x.UserId == userId;
            }

            var idArray = idEnumerable as int[] ?? idEnumerable.ToArray();

            if (!idArray.Any())
            {
                return x => x.UserId == userId;
            }

            Expression<Func<IGoalModel, bool>> filter = x => x.Id == idArray[0] && x.UserId == userId;

            for (var i = 1; i <= idArray.Length; i++)
            {
                var i1 = i;
                filter = filter.Or(x => x.Id == idArray[i1] && x.UserId == userId);
            }

            return filter;
        }
    }
}
