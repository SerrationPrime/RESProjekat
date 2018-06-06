using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMISystemManagementUI
{
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
    }
}
