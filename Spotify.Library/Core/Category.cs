using ServiceStack;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/browse/categories")]
    public class GetCategories : IReturnPagable<BrowseResponse>, IGet
    {
        public short Limit { get; set; }
        public short Offset { get; set; }
    }
    public class Category
    {
        public string Href { get; set; }

        public List<SpotifyImage> Icons { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}
