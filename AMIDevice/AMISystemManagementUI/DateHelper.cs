using OxyPlot.Axes;
using System;

namespace AMISystemManagementUI
{
    /// <summary>
    /// Kolekcija korisnih metoda za rad sa vremenom u timestamp formatu
    /// </summary>
    public static class DateHelper
    {
        public static long StartOfDay(this DateTime theDate)
        {
            return (new DateTimeOffset(theDate.Date)).ToUnixTimeSeconds();
        }

        public static long EndOfDay(this DateTime theDate)
        {
            return (new DateTimeOffset(theDate.Date.AddDays(1).AddTicks(-1))).ToUnixTimeSeconds();
        }

        public static DateTime ParseDate(string dateTimePassed)
        { 
            string[] dateTime = dateTimePassed.Trim().Split('.');
            DateTime date = new DateTime(Int32.Parse(dateTime[2]), Int32.Parse(dateTime[1]), Int32.Parse(dateTime[0]));
            return date;
        }

        /// <summary>
        /// OxyPlot koristu posebnu verziju double-a za ose svog grafika. Ovo omogucava konverziju timestamp-a na taj format
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static double TimeStampToOxyTime(long timestamp)
        {
            DateTimeOffset offsetTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            DateTime time = offsetTime.LocalDateTime;
            return DateTimeAxis.ToDouble(time);
        }
    }
}
