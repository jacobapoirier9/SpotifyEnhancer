using Spotify.Library.Core;
using Spotify.Web.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spotify.Web.Models
{
    public class TrackViewModel
    {
        public SpotifyTrack Track { get; set; }

        public List<Group> Groups { get; set; }

        public List<int> MemberedGroupIds { get; set; }
    }
}
