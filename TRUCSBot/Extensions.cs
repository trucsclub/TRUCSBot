using System;
using System.Collections.Generic;
using System.Text;

namespace TRUCSBot
{
    public static class Extensions
    {
        public static string Random(this List<string> list)
        {
            var m = new Random().Next(0, list.Count - 1);
            return list[m];
        }
    }
}
