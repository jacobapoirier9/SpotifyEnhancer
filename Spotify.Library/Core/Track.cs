using ServiceStack;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/tracks/{TrackId}")]
    public class SpotifyGetTrack : IReturn<Track>, IGet
    { 
        public string TrackId { get; set; }
    }

    [Route("/tracks")]
    public class GetTracks : IReturn<MultipleTracksWrapper>, IGet
    {
        public List<string> Ids { get; set; }
    }


    public class Track : SpotifyObject
    {
        [DataMember(Name = "artists")]
        public List<Artist> Artists { get; set; }

        [DataMember(Name = "album")]
        public Album Album { get; set; }

        [DataMember(Name = "duration_ms")]
        public int DurationMs { get; set; }

        [DataMember(Name = "popularity")]
        public byte Popularity { get; set; }

        [DataMember(Name = "preview_url")]
        public string PreviewUrl { get; set; }

        [DataMember(Name = "track_number")]
        public short TrackNumber { get; set; }
    }

    public class MultipleTracksWrapper
    {
        // Not sure what this is for.. is there another end point that potentially will use this?
        //[DataMember(Name = "track")]
        //public SpotifyTrack Track { get; set; }

        [DataMember(Name = "tracks")]
        public List<Track> Tracks { get; set; }
    }

    public class SingleTrackWrapper
    {
        public Track Track { get; set; }
    }
}
