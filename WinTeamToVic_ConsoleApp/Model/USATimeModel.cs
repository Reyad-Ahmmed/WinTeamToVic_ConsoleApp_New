using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTeamToVic_ConsoleApp.Model
{
    public static class USATimeModel
    {
        public static DateTime GetUSATime()
        {
            DateTime cstTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central Standard Time");
            return cstTime;
        }
    }
}
