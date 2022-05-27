using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    public class SpotifyAlbum : SpotifyObject
    {
        [DataMember(Name = "images")]
        public List<SpotifyImage> Images { get; set; }

        [DataMember(Name = "artists")]
        public List<SpotifyArtist> Artists { get; set; }

        [DataMember(Name = "release_date")]
        public string ReleaseDate { get; set; }

        [DataMember(Name = "release_date_precision")]
        public string ReleaseDatePrecision { get; set; }

        [DataMember(Name = "total_tracks")]
        public byte TotalTracks { get; set; }
    }
}
