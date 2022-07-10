using ServiceStack;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/artists/{Id}")]
    public class GetArtist : IReturn<FullArtist>, IGet
    {
        public string Id { get; set; }
    }

    [Route("/artists")]
    public class GetArtists : IReturn<MultipleArtistsWrapper>, IGet
    {
        [DataMember(Name = "ids")]
        public List<string> Ids { get; set; }
    }

    public class Artist : SpotifyObject
    {
    }

    public class FullArtist : Artist
    {
        [DataMember(Name = "followers")]
        public SpotifyEntityReferenceObject Followers { get; set; }

        [DataMember(Name = "genres")]
        public List<string> Genres { get; set; }

        [DataMember(Name = "images")]
        public List<SpotifyImage> Images { get; set; }

        [DataMember(Name = "popularity")]
        public byte Popularity { get; set; }
    }

    public class MultipleArtistsWrapper
    {
        [DataMember(Name = "artists")]
        public List<FullArtist> Artists { get; set; }
    }
}
