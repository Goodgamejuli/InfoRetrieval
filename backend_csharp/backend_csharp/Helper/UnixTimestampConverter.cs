using System.Globalization;
using MetaBrainz.MusicBrainz;

namespace backend_csharp.Helper
{

public static class UnixTimestampConverter
{
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }

    public static long ToUnixTimestamp(this PartialDate partialDate)
    {
        if (partialDate.Year == 0)
            return 0;

        // If month or date aren't set. Just use the first
        int month = partialDate.Month ?? 1;
        int day = partialDate.Day ?? 1;

        DateTime dateTime = new DateTime(partialDate.Year.Value, month, day, 0, 0, 0, DateTimeKind.Utc);

        return dateTime.ToUnixTimestamp();
    }

    public static DateOnly ToDateOnly(this long unixTimeSeconds)
    {
        DateTime dateTimeFromUnix = DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;

        return DateOnly.FromDateTime(dateTimeFromUnix);
    }

    public static DateSearch? ConvertStringToDateSearch(this string search)
    {
        // if Range is given
        if (search.Contains(" - "))
        {
            string[] dates = search.Split((" - "));

            if (dates.Length != 2)
                return null;

            DateTime? startDateTime = dates[0].ConvertStringToDateTime();
            DateTime? endDateTime = dates[1].ConvertStringToDateTime();

            if(startDateTime == null || endDateTime == null) return null;

            return new DateSearch()
            {
                IsRange = true,
                StartDate = startDateTime.Value.ToUnixTimestamp(),
                EndDate = endDateTime.Value.ToUnixTimestamp(),
            };
        } 
        // Find all songs of year
        else if (search.Length == 4)
        {
            DateTime? startDateTime = search.ConvertStringToDateTime();

            if (startDateTime == null)
                return null;

            return new DateSearch()
            {
                IsRange = true,
                StartDate = startDateTime.Value.ToUnixTimestamp(),
                EndDate = startDateTime.Value.ToUnixTimestamp() + 31536000, // Adding seconds of a year
            };
        }
        // Find all songs of month of a year
        else if(search.Length == 7)
        {
            DateTime? startDateTime = search.ConvertStringToDateTime();

            if (startDateTime == null)
                return null;

            return new DateSearch()
            {
                IsRange = true,
                StartDate = startDateTime.Value.ToUnixTimestamp(),
                EndDate = startDateTime.Value.ToUnixTimestamp() + 2592000, // Adding seconds of a month with 30 days
            };
        }
        // Find all songs of day 
        else if (search.Length == 10)
        {
            DateTime? startDateTime = search.ConvertStringToDateTime();

            if (startDateTime == null)
                return null;

            return new DateSearch()
            {
                IsRange = true,
                StartDate = startDateTime.Value.ToUnixTimestamp(),
                EndDate = startDateTime.Value.ToUnixTimestamp() + 86400, // Adding seconds of a day
            };
        }

        return null;

    }

    private static DateTime? ConvertStringToDateTime(this string dateString)
    {
        DateTime date;
        // Only Year is given
        if (dateString.Length == 4) 
        {
            date = new DateTime(int.Parse(dateString), 1, 1); 
        }
        // Month plus year is given. Example --> 12.2000
        else if (dateString.Length == 7 ) 
        {
            date = DateTime.ParseExact(dateString, "MM.yyyy", CultureInfo.InvariantCulture);
        }
        // Full date is given. Example --> 12.05.2000
        else if (dateString.Length == 10) 
        {
            date = DateTime.ParseExact(dateString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
        else
        {
            return null;
        }
        return date;
    }

}

}

public class DateSearch
{
    public bool IsRange;
    public long? StartDate;
    public long? EndDate;
}

