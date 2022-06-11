using ServiceStack.DataAnnotations;

namespace Spotify.Web.Database
{
    [Schema("Spotify")]
    [Alias("Groups")]
    public class Group
    {
        public int GroupId { get; set; }

        public string Username { get; set; }

        public string GroupName { get; set; }

        public string GroupDescription { get; set; }
    }
}
