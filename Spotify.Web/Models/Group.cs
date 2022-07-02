using ServiceStack.DataAnnotations;
using System.Collections.Generic;

namespace Spotify.Web.Models
{
    public class FindGroups
    {
    }

    public class GetGroup
    {
        public int GroupId { get; set; }
    }

    public class FindGroupRelationships
    {
        public List<string> ItemIds { get; set; }
    }

    public class SaveGroup
    {
        public string GroupName { get; set; }

        public string GroupDescription { get; set; }
    }


    [Schema("Spotify")]
    [Alias("FindGroups")]
    public class FindGroupsResponse
    {
        public string Username { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int TrackCount { get; set; }
        public int AlbumCount { get; set; }
        public int ArtistCount { get; set; }
    }

    [Schema("Spotify")]
    [Alias("FindRelationships")]
    public class FindRelationshipsResponse
    {
        public string Username { get; set; }

        public string RelationshipId { get; set; }

        public string GroupName { get; set; }

        public string ItemType { get; set; }

        public string ItemId { get; set; }
    }
}
