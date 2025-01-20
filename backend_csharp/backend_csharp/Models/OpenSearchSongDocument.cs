using OpenSearch.Client;

namespace backend_csharp.Models
{
/// <summary>
/// This class defines the structure of a document of a track, that shall get saved in open search.
/// </summary>
    public class OpenSearchSongDocument : IComparable <OpenSearchSongDocument>
    {
        [Text]
        public string Id { get; set; }

        [Text]
        public string Title { get; set; }

        [Text]
        public string Lyrics { get; set; }

        [Text]
        public string AlbumTitle { get; set; }

        [Keyword]
        public string ReleaseDate { get; set; }

        [Text]
        public string ArtistName { get; set; }

        [Keyword]
        public List <string> Genre { get; set; }

        public int CompareTo(OpenSearchSongDocument? other)
        {
            // How should songs be sorted when displayed?
            
            // 1. Found songs should be grouped by their artist
            //      1.a. Found artist should be alphabetically sorted
            
            // 2. Found songs should be grouped by their album
            //      2.a Found albums should be alphabetically sorted
            
            // 3. Found songs should be alphabetically sorted

            if (other == null)
                return -1;

            var artistCompare = string.Compare(ArtistName, other.ArtistName, StringComparison.Ordinal);

            if (artistCompare != 0)
                return artistCompare;
            
            var albumCompare = string.Compare(AlbumTitle, other.AlbumTitle, StringComparison.Ordinal);
            
            if (albumCompare != 0)
                return albumCompare;
            
            return string.Compare(Title, other.Title, StringComparison.Ordinal);
        }
    }
}
