using ServiceStack.DataAnnotations;

namespace Spotify.Web.Database
{
    [Schema("Spotify")]
    [Alias("GroupRelationship")]
    public class DbGroupRelationship
    {
        public string Username { get; set; }

        public int GroupId { get; set; }

        public string ItemType { get; set; }

        public string ItemId { get; set; }
    }
}
