using ServiceStack;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Spotify.Library.Core
{
    [Route("/me")]
    public class GetUser : IReturn<User>, IGet
    {

    }

    public class User : SpotifyObject
    {
        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "external_urls")]
        public ExternalUrls ExternalUrls { get; set; }

        [DataMember(Name = "followers")]
        public SpotifyEntityReferenceObject FollowersReference { get; set; }

        [DataMember(Name = "href")]
        public string Href { get; set; }

        [DataMember(Name = "product")]
        public string Product { get; set; }
    }
}
