using MetaBrainz.MusicBrainz;
using SpotifyAPI.Web;

namespace backend_csharp.Services;

public class SpotifyApiService
{
    // Client_ID and Client_Secret are important to connect to my created spotifyAPI-Project
    private const string ClientId = "5bf41112989f474dbf67f774464106ea";
    private const string ClientSecret = "801a145398684171b84ad68b16784b02";
    private static SpotifyApiService? s_instance;

    public static SpotifyApiService Instance => s_instance ??= new SpotifyApiService();

    private readonly SpotifyClient _spotifyClient;

    private SpotifyApiService()
    {
        SpotifyClientConfig config = SpotifyClientConfig.CreateDefault().
                                                         WithAuthenticator(
                                                             new ClientCredentialsAuthenticator(
                                                                 ClientId,
                                                                 ClientSecret));

        _spotifyClient = new SpotifyClient(config);
    }

    #region Public Methods

    public async Task <OpenSearchService.CrawlSongData[]?> CrawlAllSongsOfArtist(string artistName)
    {
        Console.WriteLine("Crawling songs from spotify...");
        
        FullArtist? artist = await SearchForArtist(artistName);

        if (artist == null)
            return null;

        FullAlbum[]? albums = await GetAlbumsOfArtist(artist);

        if (albums is not {Length: > 0})
            return null;

        List <OpenSearchService.CrawlSongData> output = [];

        foreach (FullAlbum album in albums)
        {
            List <SimpleTrack>? songs = album.Tracks.Items;

            if (songs is not {Count: > 0})
                continue;

            foreach (SimpleTrack song in songs)
            {
                output.Add(
                    new OpenSearchService.CrawlSongData
                    {
                        id = song.Id,
                        title = song.Name,
                        albumId = album.Id,
                        albumTitle = album.Name,
                        albumCoverUrl = album.Images[0].Url,
                        artistId = artist.Id,
                        artistName = artist.Name,
                        artistCoverUrl = artist.Images[0].Url,
                        genres = artist.Genres.ToArray(),
                        releaseDate = new PartialDate(album.ReleaseDate)
                    });
            }
        }

        return output.ToArray();
    }

    #endregion

    #region Private Methods

    private async Task <FullAlbum[]?> GetAlbumsOfArtist(FullArtist artist)
    {
        var artistAlbumRequest = new ArtistsAlbumsRequest();

        Paging <SimpleAlbum> simpleAlbums;

        try
        {
            simpleAlbums = await _spotifyClient.Artists.GetAlbums(artist.Id, artistAlbumRequest);
        }
        catch (Exception)
        {
            Console.WriteLine($"No albums found for the artist [{artist.Name}]!");

            return null;
        }

        if (simpleAlbums.Items == null || simpleAlbums.Items.Count == 0)
            return null;

        List <FullAlbum> albums = [];

        // Simple Album needs to get converted to FullAlbum, so that it is possible to read the tracks in the album
        foreach (SimpleAlbum simpleAlbum in simpleAlbums.Items)
        {
            try
            {
                albums.Add(await _spotifyClient.Albums.Get(simpleAlbum.Id));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        return albums.ToArray();
    }

    /// <summary>
    ///     Searching for an artist by his name.
    ///     Note, that therefore the searchRequest of the spotifyAPI is used.
    ///     Because to directly find an artist u need the spotifyID of this artist
    /// </summary>
    /// <param name="artistName">Name of the artist</param>
    /// <returns>Full data of the artist or null if no artist was found</returns>
    private async Task <FullArtist?> SearchForArtist(string artistName)
    {
        var searchRequest = new SearchRequest(
            SearchRequest.Types.Artist,
            artistName
        );

        try
        {
            SearchResponse artists = await _spotifyClient.Search.Item(searchRequest);

            // Only want to get the first artist, that was found
            return artists.Artists.Items?[0];
        }
        catch (Exception)
        {
            Console.WriteLine($"No artist with name {artistName} was found!");

            return null;
        }
    }

    #endregion
}
