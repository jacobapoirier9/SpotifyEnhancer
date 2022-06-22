using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    public class BrowseResponse
    {
        [DataMember(Name = "categories")]
        public PagableResponse<Category> Categories { get; set; }
    }
}
