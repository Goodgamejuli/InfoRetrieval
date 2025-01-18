using backend_csharp.Models;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Browses;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using MetaBrainz.MusicBrainz.Interfaces.Searches;

namespace backend_csharp.Services;

public static class MusicBrainzApiService
{
    #region Public Methods

    public static async Task <List <OpenSearchSongDocument>?> GetAllTracksOfArtistAsOpenSearchDocument(
        string artistName)
    {
        var query = new Query("InfoManagement", "0.0.1", "tiogiras@gmail.com");

        IArtist? artist = await query.QueryArtist(artistName);

        if (artist == null)
            return null;

        IReadOnlyList <IWork>? tracks = await query.QueryAllTracks(artist);
        IReadOnlyList <IRecording>? recordings = await query.QueryAllRecordings(artist);

        if (tracks == null || recordings == null)
            return null;

        Dictionary <string, PartialDate> recordingDates = new();
        
        foreach (IRecording recording in recordings)
        {
            if (recording.FirstReleaseDate == null)
                continue;
            
            recordingDates.Add(recording.Id.ToString(), recording.FirstReleaseDate);
        }
        
        List <OpenSearchSongDocument> output = [];

        for (var i = 0; i < tracks.Count; i++)
        {
            try
            {
                Console.WriteLine($"Generating track {i}/{tracks.Count}");
            
                IWork track = tracks[i];
            
                output.Add(CreateOpenSearchTrack(query, artist, track, recordingDates));
            }
            catch (Exception)
            {
                Console.WriteLine("Skipped entry caused by invalid mbid");
            }
        }

        return output;
    }
    
    #endregion

    #region Private Methods

    private static OpenSearchSongDocument CreateOpenSearchTrack(Query query, IArtist artist, IWork track, Dictionary <string, PartialDate> recordings)
    {
        var releaseDate = GetFirstReleaseDateForTrack(track, recordings, out Guid releaseRecording);
        
        var osSong = new OpenSearchSongDocument
        {
            Id = track.Id.ToString(),
            AlbumTitle = GetAlbumTitleOfRecording(query, releaseRecording),
            ArtistName = artist.Name ?? "Unknown Artist",
            Lyrics = "",
            Title = track.Title ?? "Unknown Title",
            ReleaseDate = releaseDate,
            Genre = track.Genres != null
                ? track.Genres.Select(genre => genre.Name ?? "Unknown Genre").ToList()
                : []
        };

        return osSong;
    }

    private static string GetAlbumTitleOfRecording(Query query, Guid releaseRecording)
    {
        IRecording recording = query.LookupRecording(releaseRecording, Include.Releases);

        IRelease? release = recording.Releases?.FirstOrDefault(release => release.Date == recording.FirstReleaseDate);
        
        return release?.Title ?? "Unknown Album";
    }

    private static string GetFirstReleaseDateForTrack(IWork track, Dictionary <string, PartialDate> recordingDates, out Guid releaseRecording)
    {
        releaseRecording = Guid.Empty;
        
        if (track.Relationships == null)
            return "";

        Guid[] recordings = track.Relationships.Where(rel => rel.Recording != null).
                                  Select(relationship => relationship.Recording!.Id).ToArray();

        PartialDate? firstReleaseDate = null;

        foreach (Guid recording in recordings)
        {
            if (!recordingDates.TryGetValue(recording.ToString(), out PartialDate? date))
                continue;

            if (firstReleaseDate != null && firstReleaseDate <= date)
                continue;

            firstReleaseDate = date;
            releaseRecording = recording;
        }

        return firstReleaseDate?.ToString() ?? "";
    }

    private static async Task <IArtist?> QueryArtist(this Query query, string artistName)
    {
        // Queries all artists corresponding to the artist name but only picks the one with the highest score
        ISearchResults <ISearchResult <IArtist>> artists = await query.FindArtistsAsync(artistName, 1, simple: true);

        return artists.TotalResults == 0 ? null : artists.Results[0].Item;
    }

    private static async Task <IReadOnlyList <IWork>?> QueryAllTracks(this Query query, IArtist artist)
    {
        List <IWork> allTracks = [];
        
        var offset = 0;

        IBrowseResults<IWork> works;
        do
        {
            if (offset > 0)
                Console.WriteLine($"...{offset}");
            
            works = await query.BrowseArtistWorksAsync(
                artist.Id,
                100,
                offset,
                Include.RecordingRelationships | Include.Genres);
            
            allTracks.AddRange(works.Results);
            offset += works.Results.Count;
        }
        while (works.Results.Count > 0);
        
        Console.WriteLine($"Queried {allTracks.Count} tracks");
        
        return allTracks.Count > 0 ? allTracks : null;
    }
    
    private static async Task <IReadOnlyList <IRecording>?> QueryAllRecordings(this Query query, IArtist artist)
    {
        List <IRecording> allRecordings = [];
        
        var offset = 0;

        IBrowseResults<IRecording> recordings;
        
        do
        {
            if (offset > 0)
                Console.WriteLine($"...{offset}");

            recordings = await query.BrowseArtistRecordingsAsync(
                artist.Id,
                100,
                offset);
            
            allRecordings.AddRange(recordings.Results);
            offset += recordings.Results.Count;
        }
        while (recordings.Results.Count > 0);
        
        Console.WriteLine($"Queried {allRecordings.Count} recordings");
        
        return allRecordings.Count > 0 ? allRecordings : null;
    }

    #endregion
}
