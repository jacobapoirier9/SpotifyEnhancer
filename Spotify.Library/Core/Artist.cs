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
        public List<string> Ids { get; set; }
    }

    public class Artist : SpotifyObject
    {
    }

    public class FullArtist : Artist
    {
        public SpotifyEntityReferenceObject Followers { get; set; }

        public List<string> Genres { get; set; }

        public List<SpotifyImage> Images { get; set; }

        public byte Popularity { get; set; }
    }

    public class MultipleArtistsWrapper
    {
        public List<FullArtist> Artists { get; set; }
    }
}
