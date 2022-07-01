using System.Collections.Generic;

namespace Spotify.Web.Models
{
    public class FindGroups : IHasUsername
    {
        public string Username { get; set; }
    }

    public class GetGroup : IHasUsername
    {
        public string Username { get; set; }

        public int GroupId { get; set; }
    }

    public class FindGroupRelationships : IHasUsername
    {
        public string Username { get; set; }

        public List<string> ItemIds { get; set; }
    }

    public class SaveGroup : IHasUsername
    {
        public string Username { get; set; }

        public string GroupName { get; set; }

        public string GroupDescription { get; set; }
    }
}
