using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/tracks/{TrackId}")]
    public class GetTrack : IReturn<Track>, IGet
    { 
        public string TrackId { get; set; }
    }

    [Route("/tracks")]
    public class GetTracks : IReturn<MultipleTracksWrapper>, IGet
    {
        [DataMember(Name = "ids")]
        public List<string> Ids { get; set; }
    }

    [Route("/audio-features/{Id}")]
    public class GetAudioFeatures : IReturn<AudioFeatures>, IGet
    {
        public string Id { get; set; }
    }

    [Route("/audio-features")]
    public class GetAudioFeaturesMultiple : IReturn<MultipleAudioFeaturesWrapper>, IGet
    {
        [DataMember(Name = "ids")]
        public List<string> Ids { get; set; }
    }

    [Route("/me/tracks/contains")]
    public class GetTrackIsLiked : IReturn<List<bool>>, IGet
    {
        [DataMember(Name = "ids")]
        public List<string> Ids { get; set; }
    }

    public class Track : SpotifyObject
    {
        private List<Artist> _allUniqueArtists;

        public List<Artist> AllUniqueArtists
        {
            get
            {
                if (_allUniqueArtists is null)
                {
                    _allUniqueArtists = new List<Artist>();

                    var all = this.Artists.Concat(this.Album.Artists);
                    foreach (var id in all.Select(a => a.Id).Distinct())
                    {
                        _allUniqueArtists.Add(all.First(a => a.Id == id));
                    }
                }

                return _allUniqueArtists;
            }
        }



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

    public class MultipleAudioFeaturesWrapper
    {
        [DataMember(Name = "audio_features")]
        public List<AudioFeatures> AudioFeatures { get; set; }
    }
    public class AudioFeatures
    {
        [DataMember(Name = "acousticness")]
        /// <summary>
        /// A confidence measure from 0.0 to 1.0 of whether the track is acoustic. 1.0 represents high confidence the track is acoustic.
        /// </summary>
        public decimal Acousticness { get; set; }

        [DataMember(Name = "analysis_url")]
        /// <summary>
        /// A URL to access the full audio analysis of this track. An access token is required to access this data.
        /// </summary>
        public string AnalysisUrl { get; set; }

        [DataMember(Name = "danceability")]
        /// <summary>
        /// Danceability describes how suitable a track is for dancing based on a combination of musical elements including tempo, 
        /// rhythm stability, beat strength, and overall regularity. A value of 0.0 is least danceable and 1.0 is most danceable.
        /// </summary>
        public decimal Danceability { get; set; }

        [DataMember(Name = "duration_ms")]
        /// <summary>
        /// The duration of the track in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }

        [DataMember(Name = "energy")]
        /// <summary>
        /// Energy is a measure from 0.0 to 1.0 and represents a perceptual measure of intensity and activity. 
        /// Typically, energetic tracks feel fast, loud, and noisy. For example, death metal has high energy, while a Bach prelude scores low on the scale. 
        /// Perceptual features contributing to this attribute include dynamic range, perceived loudness, timbre, onset rate, and general entropy.
        /// </summary>
        public decimal Energy { get; set; }

        [DataMember(Name = "id")]
        /// <summary>
        /// The Spotify ID for the track.
        /// </summary>
        public string Id { get; set; }

        [DataMember(Name = "instrumentalness")]
        /// <summary>
        /// Predicts whether a track contains no vocals. "Ooh" and "aah" sounds are treated as instrumental in this context. 
        /// Rap or spoken word tracks are clearly "vocal". The closer the instrumentalness value is to 1.0, the greater likelihood the track contains no vocal content. 
        /// Values above 0.5 are intended to represent instrumental tracks, but confidence is higher as the value approaches 1.0.
        /// </summary>
        public decimal Instrumentalness { get; set; }

        [DataMember(Name = "key")]
        /// <summary>
        /// The key the track is in. Integers map to pitches using standard Pitch Class notation. E.g. 0 = C, 1 = C♯/D♭, 2 = D, and so on. 
        /// -1 = No key detected
        /// 1 = C Sharp / D Flat
        /// 2 = D
        /// etc..
        /// </summary>
        public int Key { get; set; }

        [DataMember(Name = "liveness")]
        /// <summary>
        /// Detects the presence of an audience in the recording. Higher liveness values represent an increased probability that the track was performed live. 
        /// A value above 0.8 provides strong likelihood that the track is live.
        /// </summary>
        public decimal Liveness { get; set; }

        [DataMember(Name = "loudness")]
        /// <summary>
        /// The overall loudness of a track in decibels (dB). Loudness values are averaged across the entire track and are useful for comparing relative loudness of tracks. 
        /// Loudness is the quality of a sound that is the primary psychological correlate of physical strength (amplitude). Values typically range between -60 and 0 db.
        /// </summary>
        public decimal Loudness { get; set; }

        [DataMember(Name = "mode")]
        /// <summary>
        /// Mode indicates the modality (major or minor) of a track, the type of scale from which its melodic content is derived. Major is represented by 1 and minor is 0.
        /// 0 = Minor
        /// 1 = Major
        /// </summary>
        public byte Mode { get; set; }

        [DataMember(Name = "speechiness")]
        /// <summary>
        /// Speechiness detects the presence of spoken words in a track. The more exclusively speech-like the recording (e.g. talk show, audio book, poetry), 
        /// the closer to 1.0 the attribute value. Values above 0.66 describe tracks that are probably made entirely of spoken words. Values between 0.33 and 0.66 
        /// describe tracks that may contain both music and speech, either in sections or layered, including such cases as rap music. Values below 0.33 most likely 
        /// represent music and other non-speech-like tracks.
        /// </summary>
        public decimal Speechiness { get; set; }

        [DataMember(Name = "tempo")]
        /// <summary>
        /// The overall estimated tempo of a track in beats per minute (BPM). In musical terminology, tempo is the speed or pace of a given piece and derives 
        /// directly from the average beat duration.
        /// </summary>
        public decimal Tempo { get; set; }

        [DataMember(Name = "time_signature")]
        /// <summary>
        /// An estimated time signature. The time signature (meter) is a notational convention to specify how many beats are in each bar (or measure). 
        /// The time signature ranges from 3 to 7 indicating time signatures of "3/4", to "7/4".
        /// </summary>
        public byte TimeSignature { get; set; }

        [DataMember(Name = "track_href")]
        /// <summary>
        /// A link to the Web API endpoint providing full details of the track.
        /// </summary>
        public string TrackHref { get; set; }

        [DataMember(Name = "type")]
        /// <summary>
        /// The object type.
        /// Allowed value:"audio_features"
        /// </summary>
        public string Type { get; set; }

        [DataMember(Name = "uri")]
        /// <summary>
        /// The Spotify URI for the track.
        /// </summary>
        public string Uri { get; set; }

        [DataMember(Name = "valence")]
        /// <summary>
        /// A measure from 0.0 to 1.0 describing the musical positiveness conveyed by a track. 
        /// Tracks with high valence sound more positive (e.g. happy, cheerful, euphoric), 
        /// while tracks with low valence sound more negative (e.g. sad, depressed, angry).
        /// </summary>
        public decimal Valence { get; set; }
    }
}
