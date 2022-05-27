using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    public class SpotifyCurrentlyPlayingResponse
    {
        [DataMember(Name = "item")]
        public SpotifyTrack Item { get; set; }

        [DataMember(Name = "is_playing")]
        public bool IsPlaying { get; set; }
    }
}
