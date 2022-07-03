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
        [DataMember(Name = "ids")]
        public List<string> Ids { get; set; }
    }

    public class Album : SpotifyObject
    {
        [DataMember(Name = "images")]
        public List<SpotifyImage> Images { get; set; }

        [DataMember(Name = "artists")]
        public List<Artist> Artists { get; set; }

        [DataMember(Name = "release_date")]
        public string ReleaseDate { get; set; }

        [DataMember(Name = "release_date_precision")]
        public string ReleaseDatePrecision { get; set; }

        [DataMember(Name = "total_tracks")]
        public byte TotalTracks { get; set; }
    }

    public class MultipleAlbumsWrapper
    {
        [DataMember(Name = "albums")]
        public List<Album> Albums { get; set; }
    }
}
