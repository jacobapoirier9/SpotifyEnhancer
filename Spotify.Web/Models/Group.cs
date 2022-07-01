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
}
