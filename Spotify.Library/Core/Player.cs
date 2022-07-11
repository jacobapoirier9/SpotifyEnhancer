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
    public class GetCurrentlyPlaying : IReturn<CurrentlyPlaying>, IGet
    {

    }

    [Route("/me/player/recently-played")]
    public class GetRecentlyPlayedTracks : IReturn<PagableResponse<SingleTrackWrapper>>, IGet
    {
        public int Limit { get; set; }
    }

    public class PlaybackDevice
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public byte VolumePercent { get; set; }
    }

    public class PlaybackState
    {
        public PlaybackDevice Device { get; set; }

        public bool ShuffleState { get; set; }

        public bool RepeatState { get; set; }

        public long TimeStamp { get; set; }



        public Track Item { get; set; }

        public bool IsPlaying { get; set; }

        public long ProgressMs { get; set; }
    }

    public class CurrentlyPlaying
    {

        public string TimeStamp { get; set; }
        public CurrentlyPlayingContext Context { get; set; }

        public Track Item { get; set; }

        public bool IsPlaying { get; set; }
    }

    public class CurrentlyPlayingContext
    {
        public ExternalUrls ExternalUrls { get; set; }

        public string Href { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }
    }
}
