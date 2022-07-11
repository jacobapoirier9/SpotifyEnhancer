using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spotify.Web.Models.Database
{
    [Schema("Spotify")]
    [Alias("Groups")]
    public class DbGroup
    {
        public string Username { get; set; }

        [IgnoreOnInsert]
        public int GroupId { get; set; }

        public string GroupName { get; set; }
    }

    [Schema("Spotify")]
    [Alias("Relationships")]
    public class DbRelationship
    {
        public int GroupId { get; set; }

        [IgnoreOnInsert]
        public int RelationshipId { get; set; }

        public string TrackId { get; set; }
    }
}
