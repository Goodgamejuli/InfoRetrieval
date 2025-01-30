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

            DateTime? starDateTime = dates[0].ConvertStringToDateTime();
            DateTime? endDateTime = dates[1].ConvertStringToDateTime();

            if(starDateTime == null || endDateTime == null) return null;

            return new DateSearch()
            {
                IsRange = true,
                StartDate = starDateTime.Value.ToUnixTimestamp(),
                EndDate = endDateTime.Value.ToUnixTimestamp(),
                ExactDate = null
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
    public long? ExactDate;
}

//public async Task CreateIndexTest()
//        {
//            await _client.Indices.DeleteAsync("songs");

//            var createIndexResponse = await _client.Indices.CreateAsync("songs", c => c
//                                                                           .Map<Song>(m => m
//                                                                               .Properties(p => p
//                                                                                   .Text(t => t.Name(n => n.Title))
//                                                                                   .Text(t => t.Name(n => n.Artist))
//                                                                                   .Date(d => d.Name(n => n.ReleaseDate))
//                                                                                   .Number(n => n.Name(n => n.TrackNumber).Type(NumberType.Double))
//                                                                               )
//                                                                           )
//            );

//            Console.WriteLine(createIndexResponse.IsValid
//                                  ? "Index erfolgreich erstellt!"
//                                  : $"Fehler: {createIndexResponse.ServerError?.Error?.Reason}");
//        }

//        public async Task AddDocuments()
//        {
//            var songs = new List<Song>
//        {
//            new Song { Id = "1", Title = "Lose Yourself", Artist = "Eminem", ReleaseDate = new DateTime(2002, 10, 28), TrackNumber = ((DateTimeOffset)new DateTime(2002, 10, 28)).ToUnixTimeMilliseconds()},

//            new Song { Id = "2", Title = "Bohemian Rhapsody", Artist = "Queen", ReleaseDate = new DateTime(1975, 10, 31),TrackNumber = ((DateTimeOffset)new DateTime(1975, 10, 28)).ToUnixTimeMilliseconds()},
//            new Song { Id = "3", Title = "Smells Like Teen Spirit", Artist = "Nirvana", ReleaseDate = new DateTime(1991, 9, 10), TrackNumber = ((DateTimeOffset)new DateTime(1991, 10, 28)).ToUnixTimeMilliseconds()}
//        };

//            foreach (var song in songs)
//            {
//                var indexResponse = await _client.IndexDocumentAsync(song);
//                Console.WriteLine(indexResponse.IsValid
//                                      ? $"Song '{song.Title}' hinzugefügt!"
//                                      : $"Fehler: {indexResponse.ServerError?.Error?.Reason}");
//            }
//        }
//        public async Task<List<Song>?> SearchByDate(DateTime greaterThan)
//        {
//            long dateGreater = ((DateTimeOffset)greaterThan).ToUnixTimeMilliseconds();
//            var response = await _client.SearchAsync<Song>(s => s
//                                                               .Index("songs")
//                                                               .Query(q => q
//                                                                          .Range(r => r
//                                                                                     .Field(f => f.TrackNumber) // Das Datumsfeld
//                                                                                     .GreaterThanOrEquals(dateGreater)
//                                                                          )
//                                                               )
//            );

//            List<Song> songs = new List<Song>();
//            if (response.IsValid)
//            {
//                Console.WriteLine($"Gefundene Songs ({response.Documents.Count} Treffer):");
//                foreach (var song in response.Documents)
//                {
//                    Console.WriteLine($"{song.Title} - {song.Artist} ({song.ReleaseDate:yyyy-MM-dd})");
//                    songs.Add(song);
//                }
//            }
//            else
//            {
//                Console.WriteLine($"Fehler: {response.ServerError?.Error?.Reason}");

//                return null;
//            }

//            return songs;
//        }


//    }

//public class Song
//{
//    public string Id { get; set; }
//    public string Title { get; set; }
//    public string Artist { get; set; }
//    public DateTime ReleaseDate { get; set; }
//    public long TrackNumber { get; set; }

//    public long ToUnixTimestamp()
//    {
//        return ((DateTimeOffset)ReleaseDate).ToUnixTimeSeconds();
//    }
//}

