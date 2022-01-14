using System;
using System.Collections.Generic;

namespace TRUCSBot
{
    public static class Extensions
    {
        public static string Random(this List<string> list)
        {
            var m = new Random().Next(0, list.Count - 1);
            return list[m];
        }

        public static bool DayMonthIs(this DateTime date, int day, int month)
        {
            return date.Day == day && date.Month == month;
        }
    }
}
