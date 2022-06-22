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
        public Track Track { get; set; }

        public object GridData { get; set; }
    }
}
