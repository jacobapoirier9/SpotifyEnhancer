using ServiceStack;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/albums/{Id}")]
    public class GetAlbum : IReturn<Album>, IGet
    {
        public string Id { get; set; }
    }

    [Route("/albums")]
    public class GetAlbums : IReturn<MultipleAlbumsWrapper>, IGet
    {
        public List<string> Ids { get; set; }
    }

    public class Album : SpotifyObject
    {
        public List<SpotifyImage> Images { get; set; }

        public List<Artist> Artists { get; set; }

        public string ReleaseDate { get; set; }

        public string ReleaseDatePrecision { get; set; }

        public byte TotalTracks { get; set; }
    }

    public class MultipleAlbumsWrapper
    {
        public List<Album> Albums { get; set; }
    }
}
