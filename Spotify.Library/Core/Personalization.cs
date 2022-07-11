using ServiceStack;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Spotify.Library.Core
{
    [Route("/recommendations")]
    public class GetRecommendations : IReturn<MultipleTracksWrapper>, IGet
    {
        [Alias("seed_tracks")]
        [DataMember(Name = "seed_tracks")]
        public List<string> SeedTracks { get; set; }


        [Alias("seed_artists")]
        [DataMember(Name = "seed_artists")]
        public List<string> SeedArtists { get; set; }
    }
}
