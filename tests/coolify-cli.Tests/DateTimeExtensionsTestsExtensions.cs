using System;
using Xunit;

namespace coolify_cli.Tests
{
    public static class DateTimeExtensionsTestsExtensions
    {
        /// <summary>
        /// Extension method to test if a DateTime is in the past relative to another DateTime.
        /// </summary>
        /// <param name="dateTime">The date time to check</param>
        /// <param name="referenceDate">The reference date (defaults to DateTime.Now)</param>
        /// <returns>True if the dateTime is in the past relative to referenceDate</returns>
        public static bool IsInPast(this DateTime dateTime, DateTime? referenceDate = null)
        {
            var refDate = referenceDate ?? DateTime.Now;
            return dateTime < refDate;
        }

        /// <summary>
        /// Extension method to test if a DateTime is in the future relative to another DateTime.
        /// </summary>
        /// <param name="dateTime">The date time to check</param>
        /// <param name="referenceDate">The reference date (defaults to DateTime.Now)</param>
        /// <returns>True if the dateTime is in the future relative to referenceDate</returns>
        public static bool IsInFuture(this DateTime dateTime, DateTime? referenceDate = null)
        {
            var refDate = referenceDate ?? DateTime.Now;
            return dateTime > refDate;
        }

        /// <summary>
        /// Extension method to calculate the difference in business days between two dates.
        /// Business days are Monday through Friday.
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>Number of business days between the dates (inclusive)</returns>
        public static int BusinessDaysUntil(this DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return 0;
            }

            int days = 0;
            var current = startDate;

            while (current <= endDate)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    days++;
                }
                current = current.AddDays(1);
            }

            return days;
        }

        /// <summary>
        /// Extension method to get the first day of the quarter for a given date.
        /// </summary>
        /// <param name="dateTime">The date time</param>
        /// <returns>The first day of the quarter</returns>
        public static DateTime FirstDayOfQuarter(this DateTime dateTime)
        {
            int quarter = (dateTime.Month - 1) / 3 + 1;
            int month = (quarter - 1) * 3 + 1;
            return new DateTime(dateTime.Year, month, 1);
        }
    }
}