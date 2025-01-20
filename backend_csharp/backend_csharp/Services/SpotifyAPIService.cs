using backend_csharp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using SpotifyAPI.Web;

namespace backend_csharp.Services
{
    public class SpotifyAPIService
    {
        private static SpotifyAPIService _instance;
        public static SpotifyAPIService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SpotifyAPIService();
                }
                return _instance;
            }
        }

        // Client_ID and Client_Secret are important to connect to my created spotifyAPI-Project
        // TODO: Maybe don't hardcode it but outsource the data in an external file
        private const string CLIENT_ID = "5bf41112989f474dbf67f774464106ea";
        private const string CLIENT_SECRET = "801a145398684171b84ad68b16784b02";

        private SpotifyClient _spotifyClient;

        public SpotifyAPIService()
        {
            var config = SpotifyClientConfig.CreateDefault()
                                             .WithAuthenticator(
                                                  new ClientCredentialsAuthenticator(CLIENT_ID, CLIENT_SECRET));

            _spotifyClient = new SpotifyClient(config);
        }

        /// <summary>
        /// Return a track by using the spotifyAPI and the trackId
        /// </summary>
        /// <param name="id">Id of the track in the spotifyAPI</param>
        /// <returns>Track with all data</returns>
        public async Task<FullTrack> GetTrackById(string id)
        {
            FullTrack track;
            try
            {
                track = await _spotifyClient.Tracks.Get(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                track = null;
            }

            return track;
        }

        public async Task <Tuple<FullArtist, List<FullAlbum>, List <OpenSearchSongDocument>>> GetAllTracksOfArtistAsOpenSearchDocument(string artistName)
        {
            var artist = await SearchForArtist(artistName);

            if (artist == null)
                return null;

            List <FullAlbum> albumsOfArtist = await GetAlbumsOfArtist(artist);

            if (albumsOfArtist == null || albumsOfArtist.Count == 0)
                return null;

            List <OpenSearchSongDocument> allTracksOfArtist = new List <OpenSearchSongDocument>();

            await AddTracksOfAlbumToList(albumsOfArtist[0], artist, allTracksOfArtist);
            //foreach (FullAlbum album in albumsOfArtist)
            //{
            //    await AddTracksOfAlbumToList(album, artist, allTracksOfArtist);
            //}

            return new Tuple <FullArtist, List <FullAlbum>, List <OpenSearchSongDocument>>(
                artist,
                albumsOfArtist,
                allTracksOfArtist);
        }

        /// <summary>
        /// Tries to find the given song and artist in the spotify-api to retrieve the specific Id
        /// </summary>
        /// <param name="title">Title of the song</param>
        /// <param name="artist">Name of the artist</param>
        public async Task<string> GetSpotifyIdOfSong(string? title, string? artist)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist))
                return string.Empty;
            
            var searchRequest = new SearchRequest(
                SearchRequest.Types.Track,
                query: title
            );

            FullTrack? song = null;

            try
            {
                var songs = await _spotifyClient.Search.Item(searchRequest);

                // Only wanna get the first artist, that was found
                song = songs.Tracks.Items.FirstOrDefault(song => 
                                                             song.Artists.Any(simpleArtist => 
                                                                                  simpleArtist.Name.ToLower().Equals(artist.ToLower())));
            }
            catch (Exception) { }

            return song == null ? string.Empty : song.Id;
        }


        private async Task AddTracksOfAlbumToList(FullAlbum album, FullArtist artist, List <OpenSearchSongDocument> tracks)
        {
            foreach (var track in album.Tracks.Items)
            {
                var lyric = await LyricsOvhService.Instance.GetLyricByArtistAndTitle(artist.Name, track.Name);
                
                if(string.IsNullOrEmpty(lyric))
                    continue;

                var osTrack = CreateOpenSearchTrack(track, album, artist, lyric);
                tracks.Add(osTrack);
            }
        }

        private OpenSearchSongDocument CreateOpenSearchTrack(SimpleTrack track, FullAlbum album, FullArtist artist, string lyrics)
        {
            OpenSearchSongDocument osSong = new OpenSearchSongDocument
            {
                Id = track.Id,
                AlbumTitle = album.Name,
                ArtistName = artist.Name,
                Lyrics = lyrics,
                Title = track.Name,
                ReleaseDate = album.ReleaseDate,
                Genre = artist.Genres
            };

            return osSong;
        }

        /// <summary>
        /// Searching for an artist by his name.
        /// Note, that therefore the searchRequest of the spotifyAPI is used.
        /// Because to directly find an artist u need the spotifyID of this artist
        /// </summary>
        /// <param name="artistName">Name of the artist</param>
        /// <returns>Full data of the artist or null if no artist was found</returns>
        private async Task<FullArtist?> SearchForArtist(string artistName)
        {
            SearchRequest searchRequest = new SearchRequest(
                SearchRequest.Types.Artist,
                query: artistName
            );

            FullArtist? artist;

            try
            {
                var artists = await _spotifyClient.Search.Item(searchRequest);

                // Only wanna get the first artist, that was found
                artist = artists.Artists?.Items[0];
            }
            catch (Exception e)
            {
                Console.WriteLine($"No artist with name {artistName} was found!");
                artist = null;
            }
            return artist;
        }

        private async Task <List<FullAlbum>> GetAlbumsOfArtist(FullArtist artist)
        {
            var artistAlbumRequest = new ArtistsAlbumsRequest();

            Paging <SimpleAlbum> simpleAlbums;

            try
            {
                simpleAlbums = await _spotifyClient.Artists.GetAlbums(artist.Id, artistAlbumRequest);
            }
            catch (Exception e)
            {
                Console.WriteLine($"No album was found for artist {artist.Name}!");

                return null;
            }

            List <FullAlbum> albums = new List <FullAlbum>();

            // Simple Album needs to get converted to FullAlbum, so that it is possible to read the tracks in the album
            foreach (SimpleAlbum simpleAlbum in simpleAlbums.Items)
            {
                try
                {
                    var album = await _spotifyClient.Albums.Get(simpleAlbum.Id);

                    if (album != null)
                        albums.Add(album);
                }
                catch (Exception e)
                {
                    continue;
                }
            }

            return albums;
        }
    }
}
