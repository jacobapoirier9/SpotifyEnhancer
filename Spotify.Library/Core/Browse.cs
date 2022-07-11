using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    public class BrowseResponse
    {
        public PagableResponse<Category> Categories { get; set; }
    }
}
