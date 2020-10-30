using System;

namespace ePiggyWeb.Utilities
{
    public static class TimeManager
    {
        public static readonly DateTime OneMonthAhead = GetEndOfMonth(DateTime.Today.AddMonths(1));

        public static int DifferenceInMonths(DateTime laterTime, DateTime earlierTime)
        {
            return ((laterTime.Year - earlierTime.Year) * 12) + laterTime.Month - earlierTime.Month;
        }

        public static DateTime GetBeginningOfMonth(DateTime dateTime)
        {
            return ChangeDay(dateTime, 0);
        }

        public static DateTime GetEndOfMonth(DateTime dateTime)
        {
            var day = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            return ChangeDay(dateTime, day);
        }

        public static DateTime ChangeYear(DateTime dateTime, int newYear)
        {
            var temp = dateTime.AddYears(newYear - dateTime.Year);
            return temp;
        }

        public static DateTime ChangeMonth(DateTime dateTime, int newMonth)
        {
            var temp = dateTime.AddMonths(newMonth - dateTime.Month);
            return temp;
        }

        public static DateTime ChangeDay(DateTime dateTime, int newDay)
        {
            var temp = dateTime.AddDays(newDay - dateTime.Day);
            return temp;
        }

        public static DateTime MoveToNextMonth(DateTime dateTime)
        {
            return ChangeMonth(dateTime, dateTime.Month + 1);
        }

        public static DateTime MoveToPreviousMonth(DateTime dateTime)
        {
            return ChangeMonth(dateTime, dateTime.Month - 1);
        }

        public static DateTime MoveToNextYear(DateTime dateTime)
        {
            return ChangeYear(dateTime, dateTime.Year + 1);
        }

        public static DateTime MoveToPreviousYear(DateTime dateTime)
        {
            return ChangeYear(dateTime, dateTime.Year - 1);

        }


        public static DateTime ChangeYearLimited(DateTime dateTime, int newYear)
        {
            var temp = dateTime.AddYears(newYear - dateTime.Year);
            return temp >= OneMonthAhead ? dateTime : temp;
        }

        public static DateTime ChangeMonthLimited(DateTime dateTime, int newMonth)
        {
            var temp = dateTime.AddMonths(newMonth - dateTime.Month);
            return temp >= OneMonthAhead ? dateTime : temp;
        }

        public static DateTime ChangeDayLimited(DateTime dateTime, int newDay)
        {
            var temp = dateTime.AddDays(newDay - dateTime.Day);
            return temp >= OneMonthAhead ? dateTime : temp;
        }

        public static DateTime MoveToNextMonthLimited(DateTime dateTime)
        {
            return ChangeMonthLimited(dateTime, dateTime.Month + 1);
        }

        public static DateTime MoveToPreviousMonthLimited(DateTime dateTime)
        {
            return ChangeMonthLimited(dateTime, dateTime.Month - 1);
        }

        public static DateTime MoveToNextYearLimited(DateTime dateTime)
        {
            return ChangeYearLimited(dateTime, dateTime.Year + 1);
        }

        public static DateTime MoveToPreviousYearLimited(DateTime dateTime)
        {
            return ChangeYearLimited(dateTime, dateTime.Year - 1);
        }
    }
}
