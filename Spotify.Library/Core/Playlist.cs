using ServiceStack;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/me/playlists")]
    public class SpotifyGetUserPlaylists2 : IReturnPagable<SpotifyPagableResponse<SpotifyPlaylist>>, IGet
    {
        public short Limit { get; set; }
        public short Offset { get; set; }
    }

    public class SpotifyPlaylist : SpotifyObject
    {
        [DataMember(Name = "collaborative")]
        public bool Collaborative { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "external_urls")]
        public SpotifyExternalUrlsObject ExternalUrls { get; set; }

        [DataMember(Name = "href")]
        public string Href { get; set; }

        [DataMember(Name = "images")]
        public List<SpotifyImage> Images { get; set; }

        [DataMember(Name = "public")]
        public bool Public { get; set; }

        [DataMember(Name = "tracks")]
        public SpotifyEntityReferenceObject TracksReference { get; set; }

        [DataMember(Name = "snapshot_id")]
        public string SnapshotId { get; set; }
    }
}
