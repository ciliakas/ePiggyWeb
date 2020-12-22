using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace ePiggyWeb.Utilities
{
    public static class TimeManager
    {
        public static DateTime RefreshTime()
        {
            var time = DateTime.UtcNow;

            if (time.Hour >= 16)
            {
                time = time.AddDays(1);
            }

            time = ChangeHour(time, 16);

            return time;
        }

        public static bool IsDateThisMonthAndLater(DateTime date)
        {
            return date.Year == DateTime.UtcNow.Year && date.Month >= DateTime.UtcNow.Month || date.Year > DateTime.UtcNow.Year;
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

        public static void SetDate(DateTime startDate, DateTime endDate, HttpResponse response)
        {
            if (startDate > endDate) throw new ArgumentException();

            var cookieOptions = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddHours(4) };
            response.Cookies.Append("StartDate", startDate.ToString(CultureInfo.InvariantCulture), cookieOptions);
            response.Cookies.Append("EndDate", endDate.ToString(CultureInfo.InvariantCulture), cookieOptions);
        }

        public static int DifferenceInMonths(DateTime laterTime, DateTime earlierTime)
        {
            return (laterTime.Year - earlierTime.Year) * 12 + laterTime.Month - earlierTime.Month;
        }

        private static DateTime ChangeHour(DateTime dateTime, int newHour)
        {
            return dateTime.AddHours(newHour - dateTime.Hour);
        }

        private static DateTime ChangeMonth(DateTime dateTime, int newMonth)
        {
            var temp = dateTime.AddMonths(newMonth - dateTime.Month);
            return temp;
        }

        public static DateTime MoveToNextMonth(DateTime dateTime)
        {
            return ChangeMonth(dateTime, dateTime.Month + 1);
        }
    }
}
