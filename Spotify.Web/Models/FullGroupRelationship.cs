using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spotify.Web.Models
{
    public class FullGroupRelationship
    {
        public string Username { get; set; }

        public int GroupId { get; set; }

        public string ItemType { get; set; }

        public string ItemId { get; set; }
        public string GroupName { get; set; }
    }
}
