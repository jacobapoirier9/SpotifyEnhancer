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
        public List<string> SeedTracks { get; set; }


        public List<string> SeedArtists { get; set; }
    }
}
