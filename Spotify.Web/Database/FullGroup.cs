using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spotify.Web.Database
{
    public class FullGroup
    {
        public int GroupId { get; set; }

        public string GroupName { get; set; }

        public string GroupDescription { get; set; }

        public int TrackCount { get; set; }

        public int AlbumCount { get; set; }

        public int ArtistCount { get; set; }
    }
}
