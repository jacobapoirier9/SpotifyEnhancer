using ServiceStack;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/browse/categories")]
    public class SpotifyFindCategories : IReturnPagable<SpotifyBrowseResponse>, IGet
    {
        public short Limit { get; set; }
        public short Offset { get; set; }
    }
    public class SpotifyCategory
    {
        [DataMember(Name = "href")]
        public string Href { get; set; }

        [DataMember(Name = "icons")]
        public List<SpotifyImage> Icons { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
