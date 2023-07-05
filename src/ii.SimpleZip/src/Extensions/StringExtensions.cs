using System;

namespace ii.SimpleZip.Extensions
{
    internal static class StringExtensions
    {
        public static Int32 ToDOSDateTime(this DateTime datetime)
        {
            datetime = datetime.ToLocalTime();

            // bit 00-04: second
            // bit 05-10: minute
            // bit 11-15: hour
            // bit 16-20: day
            // bit 21-24: month
            // bit 25-31: years from 1980

            var date = (ushort)(datetime.Day) | (ushort)(datetime.Month) << 5 | (ushort)(datetime.Year - 1980) << 9;
            var time = (ushort)(datetime.Second / 2) | (ushort)(datetime.Minute) << 5 | (ushort)(datetime.Hour) << 11;
            var result = (date << 16) | time;

            return result;
        }
    }
}