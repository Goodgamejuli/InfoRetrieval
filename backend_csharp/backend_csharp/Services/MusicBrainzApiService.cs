using backend_csharp.Models;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Browses;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using MetaBrainz.MusicBrainz.Interfaces.Searches;

namespace backend_csharp.Services;

public static class MusicBrainzApiService
{
    private const string ClientId = "4fhurjmg7-w6KsLo3v7zWcZyTGunYOdo";
    private const string ClientSecret = "5uUziSnDiL2TL5ixolfJYHgc10eOjdxX";

    #region Public Methods

    public static async Task <List <OpenSearchSongDocument>?> GetAllTracksOfArtistAsOpenSearchDocument(
        string artistName)
    {
        var query = new Query("InfoManagement", "0.0.1", "tiogiras@gmail.com");

        IArtist? artist = await query.QueryArtist(artistName);
        
        if (artist == null)
            return null;

        var releases = await query.QueryReleases(artist);

        foreach (IRelease release in releases)
        {
            var rels = release.Relationships;
            
            Console.WriteLine(releases);
            Console.WriteLine("--------------------------");
        }

        /*IReadOnlyList <IWork>? tracks = await query.QueryTracks(artist);
        IReadOnlyList <IRecording>? recordings = await query.QueryRecordings(artist);*/

        /*if (tracks == null || recordings == null)
            return null;*/

        /*List <OpenSearchSongDocument> output = [];

        foreach (var track in tracks)
        {
            Console.WriteLine($"---------{track.Title}-----------");

            //IRecording? recording = FindEarliestRecordingForTrack(track, recordings);

            Console.WriteLine(track.Relationships?[0].Recording?.FirstReleaseDate);
            
            output.Add(CreateOpenSearchTrack(artist, track));
        }*/

        return null;
    }

    private static IRecording? FindEarliestRecordingForTrack(IWork track, IReadOnlyList<IRecording> recordings)
    {
        if (track.Relationships == null)
            return null;
        
        Console.WriteLine($"Track contains {track.Relationships[0].Recording?.Relationships?[0].Release?.Date} relationships");

        IEnumerable <IRecording> trackRecordings =
            recordings.Where(recording => track.Relationships.Any(rel => rel.Recording.Id == recording.Id));
        
        if (!trackRecordings.Any())
            return null;

        Console.WriteLine($"Choosing from {trackRecordings.Count()} recordings");
        
        IRecording? output = trackRecordings.FirstOrDefault(rec => rec.Id.Equals(track.Relationships[0].Recording?.Id));

        for (var i = 1; i < track.Relationships.Count; i++)
        {
            IRelationship relationship = track.Relationships[i];
            IRecording? recording = trackRecordings.FirstOrDefault(rec => rec.Id.Equals(relationship.Recording?.Id));

            switch (output)
            {
                case null when recording != null:
                    output = recording;
                
                    continue;

                case null:
                    continue;
            }

            if (output.FirstReleaseDate == null && recording?.FirstReleaseDate != null)
            {
                output = recording;
                
                continue;
            }
            
            if (output.FirstReleaseDate < recording?.FirstReleaseDate)
                continue;
            
            output = recording;
        }

        return output;
    }

    #endregion

    #region Private Methods

    private static OpenSearchSongDocument CreateOpenSearchTrack(IArtist artist, IWork track/*, IRecording? recording*/)
    {
        var osSong = new OpenSearchSongDocument
        {
            Id = track.Id.ToString(),
            AlbumTitle = "MISSING Album Title",
            ArtistName = artist.Name ?? "Unknown Artist",
            Lyrics = "",
            Title = track.Title ?? "Unknown Title",
            ReleaseDate = track.Relationships?.FirstOrDefault(rel => rel.Recording != null)?.Recording?.FirstReleaseDate?.ToString() ?? string.Empty,
            Genre = track.Genres != null
                ? track.Genres.Select(genre => genre.Name ?? "Unknown Genre").ToList()
                : []
        };

        return osSong;
    }

    private static async Task <IArtist?> QueryArtist(this Query query, string artistName)
    {
        // Queries all artists corresponding to the artist name but only picks the one with the highest score
        ISearchResults <ISearchResult <IArtist>> artists = await query.FindArtistsAsync(artistName, 1, simple: true);

        return artists.TotalResults == 0 ? null : artists.Results[0].Item;
    }

    private static async Task <IReadOnlyList <IWork>?> QueryTracks(this Query query, IArtist artist)
    {
        // Queries all works (tracks) of the given artist the highest limit of queryable items is 100
        IBrowseResults <IWork> tracks = await query.BrowseArtistWorksAsync(
            artist.Id,
            100,
            null,
            Include.RecordingRelationships | Include.Genres);

        Console.WriteLine($"Queried {tracks.TotalResults} tracks");
        
        return tracks.TotalResults == 0 ? null : tracks.Results;
    }
    
    private static async Task <IReadOnlyList <IRecording>?> QueryRecordings(this Query query, IArtist artist)
    {
        // Queries all recordings (tracks) of the given artist the highest limit of queryable items is 100
        IBrowseResults <IRecording> recordings = await query.BrowseArtistRecordingsAsync(artist.Id, null, null/*, Include.ReleaseRelationships*/);

        Console.WriteLine($"Queried {recordings.Results.Count} recordings");
        
        return recordings.TotalResults == 0 ? null : recordings.Results;
    }
    
    private static async Task <IReadOnlyList <IRelease>?> QueryReleases(this Query query, IArtist artist)
    {
        // Queries all recordings (tracks) of the given artist the highest limit of queryable items is 100
        IBrowseResults <IRelease> releases = await query.BrowseArtistReleasesAsync(artist.Id, null, null, Include.RecordingRelationships | Include.WorkRelationships);

        Console.WriteLine($"Queried {releases.Results.Count} recordings");
        
        return releases.TotalResults == 0 ? null : releases.Results;
    }

    #endregion
}
