using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace ePiggyWeb.Utilities
{
    public static class TimeManager
    {
        public static DateTime OneMonthAhead { get; }= GetEndOfTheMonth(DateTime.UtcNow.AddMonths(1));

        public static DateTime LocalRefreshTime()
        {
            var time = DateTime.UtcNow;

            if (time.Hour >= 16)
            {
                time = time.AddDays(1);
            }

            time = ChangeHour(time, 16);

            return time.ToLocalTime();
        }

        public static bool IsDateThisMonthAndLater(DateTime date)
        {
            return (date.Year == DateTime.UtcNow.Year && date.Month >= DateTime.UtcNow.Month) || date.Year > DateTime.UtcNow.Year;
        }

        public static void GetDate(HttpRequest request, out DateTime pageStartDate, out DateTime pageEndDate)
        {
            var startDateCookie = request.Cookies["StartDate"];
            var endDateCookie = request.Cookies["EndDate"];

            if (startDateCookie is null || endDateCookie is null)
            {
                var today = DateTime.Now;
                pageStartDate = new DateTime(today.Year, today.Month, 1);
                pageEndDate = DateTime.Today;
            }
            else
            {
                if (DateTime.TryParse(startDateCookie, out var tempStartDate) &&
                    DateTime.TryParse(endDateCookie, out var tempEndDateTime))
                {
                    pageStartDate = tempStartDate;
                    pageEndDate = tempEndDateTime;
                }
                else
                {
                    var today = DateTime.Now;
                    pageStartDate = new DateTime(today.Year, today.Month, 1);
                    pageEndDate = DateTime.Today;
                }
            }
        }

        public static void SetDate(DateTime startDate, DateTime endDate, ref string errorMessage, HttpResponse response, HttpRequest request, out DateTime pageStartDate, out DateTime pageEndDate)
        {
            if (startDate > endDate)
            {
                errorMessage = "Start date is bigger than end date!";
                GetDate(request, out pageStartDate, out pageEndDate);
            }
            else
            {
                pageStartDate = startDate;
                pageEndDate = endDate;
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddHours(4)
                };
                response.Cookies.Append("StartDate", startDate.ToString(CultureInfo.InvariantCulture), cookieOptions);
                response.Cookies.Append("EndDate", endDate.ToString(CultureInfo.InvariantCulture), cookieOptions);
            }
        }

        public static int DifferenceInMonths(DateTime laterTime, DateTime earlierTime)
        {
            return ((laterTime.Year - earlierTime.Year) * 12) + laterTime.Month - earlierTime.Month;
        }

        public static DateTime GetBeginningOfTheMonth(DateTime dateTime)
        {
            return ChangeDay(dateTime, 0);
        }

        public static DateTime GetEndOfTheMonth(DateTime dateTime)
        {
            var day = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            return ChangeDay(dateTime, day);
        }

        public static DateTime ChangeHour(DateTime dateTime, int newHour)
        {
            return dateTime.AddHours(newHour - dateTime.Hour);
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
