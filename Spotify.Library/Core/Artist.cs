using ServiceStack;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/artists/{Id}")]
    public class GetArtist : IReturn<Artist>, IGet
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

    public class MultipleArtistsWrapper
    {
        [DataMember(Name = "artists")]
        public List<Artist> Artists { get; set; }
    }
}
