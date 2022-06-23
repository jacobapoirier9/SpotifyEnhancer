using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spotify.Library.Core;
using Spotify.Library.Services;
using Spotify.Web.Database;
using Spotify.Web.Models;
using Spotify.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spotify.Web.Controllers
{
    [Authorize]
    public class SpotifyController : Controller
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceClient _spotify;
        private readonly IDbConnectionFactory _factory;
        private readonly ICustomCache _cache;


        public SpotifyController(IServiceClient spotify, IDbConnectionFactory factory, ICustomCache cache)
        {
            _spotify = spotify;
            _factory = factory;
            _cache = cache;
        }

        public IActionResult Index()
        {
            return View("PlaylistBuilder");
        }





        //[HttpGet]
        //public List<Group> GetUserGroups(string username)
        //{
        //    using (var db = _factory.OpenDbConnection())
        //    {
        //        //var username = this.Claim<string>(Names.Username);
        //        var query = db.From<Group>()
        //            .Where(g => g.Username == username);

        //        var groups = db.Select(query);

        //        _logger.Debug("Returning {Count} groups for user {User}", groups.Count, username);

        //        return groups;
        //    }
        //}

        [HttpGet]
        [HttpPost]
        public IActionResult RecentlyPlayed()
        {
            SetupApi(out var username);
            var recentlyPlayed = _spotify.Get(new GetRecentlyPlayedTracks() { Limit = 50 });
            return Json(recentlyPlayed);
        }






        private void SetupApi() => SetupApi(out _);
        private void SetupApi(out string username)
        {
            username = this.Claim<string>(Names.Username);
            var token = this.Claim<string>(Names.AccessToken);

            _spotify.BearerToken = token;

            _logger.Trace("{Username} is using the API with token {Token}", username, token);
        }

        [Obsolete("Remove the Categories action method")]
        [HttpGet]
        public IActionResult Categories()
        {
            SetupApi();

            var categories = _spotify.GetAll(new GetCategories(), response => response.Categories)
                .OrderBy(c => c.Name);

            var grid = Helpers.ToArrayGrid(categories, 4);
            return View(grid);
        }


        [HttpGet]
        public IActionResult GetCurrentlyPlaying()
        {
            SetupApi(out var username);

            var playbackState = _spotify.Get(new GetPlaybackState());

            // This might also return the url to a playlist in the context object, if it's playing form a playlist.
            // This can be used to "Add to currently playing playlist" feature.
            var currentlyPlaying = _spotify.Get(new GetCurrentlyPlaying());

            return Json(playbackState);
        }

        [HttpGet]
        public IActionResult Track(string trackId)
        {
            SetupApi(out var username);

            var track = _spotify.Get(new GetTrack { TrackId = trackId });

            JakeLoadItemIntoDatabase(track);

            _cache.Save(username, track);

            return View("TrackView", new TrackViewModel
            {
                Track = track
            });
        }


        [HttpPost]
        public IActionResult GetGroupsForTrack()
        {
            var track = _cache.Get<Track>(this.Claim<string>(Names.Username));

            using(var db = _factory.OpenDbConnection())
            {
                var itemIds = new List<string>() { track.Id, track.Album.Id };
                itemIds.AddRange(track.Artists.Select(a => a.Id));
                itemIds.AddRange(track.Album.Artists.Select(a => a.Id));

                var query = db.From<GroupRelationship>()
                    .Join<Group>((gr, g) => gr.GroupId == g.GroupId)
                    .Where<GroupRelationship>(gr => Sql.In(gr.ItemId, itemIds));

                var results = db.Select<FullGroupRelationship>(query);

                return Json(results.Select(r => new
                {
                    r.GroupId,
                    r.GroupName,
                    r.ItemType,
                    r.ItemId,
                    AddedTo = r.ItemType switch
                    {
                        "track" => track.Name + " (track)",
                        "album" => track.Album.Name + " (album)",
                        "artist" => track.AllUniqueArtists.Select(artist => artist.Name).Join(",") + " (artist)",

                        _ => "default"
                    }

                }));
            }
        }













        private void JakeLoadItemIntoDatabase(Track track)
        {
            var itemIds = new List<string>() { track.Id, track.Album.Id };
            itemIds.AddRange(track.AllUniqueArtists.Select(a => a.Id));

            var groupIds = new List<int>() { 1, 2, 3, 4 };

            var toInsert = new List<GroupRelationship>();

            toInsert.AddRange(groupIds.Select(groupId => new GroupRelationship 
            { 
                GroupId = groupId,
                Username = "jacobapoirier9",
                ItemId = track.Id,
                ItemType = "track"
            }));

            toInsert.AddRange(groupIds.Select(groupId => new GroupRelationship
            {
                GroupId = groupId,
                Username = "jacobapoirier9",
                ItemId = track.Album.Id,
                ItemType = "album"
            }));

            foreach (var artist in track.AllUniqueArtists)
            {
                toInsert.AddRange(groupIds.Select(groupId => new GroupRelationship
                {
                    GroupId = groupId,
                    Username = "jacobapoirier9",
                    ItemId = artist.Id,
                    ItemType = "artist"
                }));
            }

            using (var db = _factory.OpenDbConnection())
            {
                db.Delete<GroupRelationship>(gr => Sql.In(gr.ItemId, itemIds));
                db.InsertAll(toInsert);
            }
        }
    }
}
