using ServiceStack.DataAnnotations;
using Spotify.Library.Core;

namespace Spotify.Web.Database
{
    [Schema("Spotify")]
    [Alias("GroupRelationship")]
    public class DbGroupRelationship
    {
        public string Username { get; set; }

        public int GroupId { get; set; }

        public ItemType ItemType { get; set; }

        public string ItemId { get; set; }
    }
}
