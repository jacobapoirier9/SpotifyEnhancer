using ServiceStack;
using ServiceStack.DataAnnotations;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/me/player")]
    public class GetPlaybackState : IReturn<PlaybackState>, IGet
    {

    }

    [Route("/me/player/currently-playing")]
    public class GetCurrentlyPlaying : IReturnVoid, IGet
    {

    }

    [Route("/me/player/recently-played")]
    public class GetRecentlyPlayed : IReturn<PagableResponse<SingleTrackWrapper>>, IGet
    {
        [DataMember(Name = "limit")]
        public int Limit { get; set; }
    }

    public class PlaybackDevice
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "volume_percent")]
        public byte VolumePercent { get; set; }
    }

    public class PlaybackState
    {
        [DataMember(Name = "device")]
        public PlaybackDevice Device { get; set; }

        [DataMember(Name = "shuffle_state")]
        public bool ShuffleState { get; set; }

        [DataMember(Name = "repeat_state")]
        public bool RepeatState { get; set; }

        [DataMember(Name = "timestamp")]
        public long TimeStamp { get; set; }



        [DataMember(Name = "item")]
        public Track Item { get; set; }

        [DataMember(Name = "is_playing")]
        public bool IsPlaying { get; set; }

        [DataMember(Name = "progress_ms")]
        public long ProgressMs { get; set; }
    }

    public class CurrentlyPlaying
    {

        [DataMember(Name = "timestamp")]
        public string TimeStamp { get; set; }
        [DataMember(Name = "context")]
        public CurrentlyPlayingContext Context { get; set; }
    }

    public class CurrentlyPlayingContext
    {
        [DataMember(Name = "external_urls")]
        public ExternalUrls ExternalUrls { get; set; }

        [DataMember(Name = "href")]
        public string Href { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "uri")]
        public string Uri { get; set; }
    }
}
