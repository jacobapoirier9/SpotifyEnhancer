using ServiceStack;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/me/playlists")]
    public class GetPlaylists : IReturnPagable<PagableResponse<Playlist>>, IGet
    {
        public short Limit { get; set; }
        public short Offset { get; set; }
    }

    [Route("/playlists/{PlaylistId}/tracks")]
    public class GetPlaylistItems : IReturnPagable<PagableResponse<SingleTrackWrapper>>, IGet
    {
        public string PlaylistId { get; set; }
        public short Limit { get; set; }
        public short Offset { get; set; }
    }

    public class Playlist : SpotifyObject
    {
        public bool Collaborative { get; set; }

        public string Description { get; set; }

        public ExternalUrls ExternalUrls { get; set; }

        public string Href { get; set; }

        public List<SpotifyImage> Images { get; set; }

        public bool Public { get; set; }

        [DataMember(Name = "tracks")]
        public SpotifyEntityReferenceObject TracksReference { get; set; }

        public string SnapshotId { get; set; }
    }
}
