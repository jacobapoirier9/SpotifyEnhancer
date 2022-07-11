using Spotify.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spotify.Web.Models
{
    public class Group
    {
        public string Username { get; set; }

        public int? GroupId { get; set; }

        public string GroupName { get; set; }

        public bool? IsMember { get; set; }

        public int? TrackCount { get; set; }
    }
}
