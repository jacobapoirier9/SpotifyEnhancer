using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    public class SpotifyBrowseResponse
    {
        [DataMember(Name = "categories")]
        public SpotifyPagableResponse<SpotifyCategory> Categories { get; set; }
    }
}
