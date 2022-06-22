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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spotify.Web.Controllers
{
    public class FindTracks
    {
        public string TrackId { get; set; }

        public bool RecentlyPlayed { get; set; }
    }
    
    [Authorize]
    public class SpotifyController : Controller
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceClient _spotify;
        private readonly IDbConnectionFactory _factory;

        public SpotifyController(IServiceClient spotify, IDbConnectionFactory factory)
        {
            _spotify = spotify;
            _factory = factory;
        }

        public IActionResult Index()
        {
            return View("PlaylistBuilder");
        }





        #region StreamRoller Database Access
        [HttpGet]
        public List<Group> GetUserGroups(string username)
        {
            using (var db = _factory.OpenDbConnection())
            {
                //var username = this.Claim<string>(Names.Username);
                var query = db.From<Group>()
                    .Where(g => g.Username == username);

                var groups = db.Select(query);

                _logger.Debug("Returning {Count} groups for user {User}", groups.Count, username);

                return groups;
            }
        }

        [HttpGet]
        [HttpPost]
        public IActionResult RecentlyPlayed()
        {
            SetupApi(out var username);
            var recentlyPlayed = _spotify.Get(new GetRecentlyPlayed() { Limit = 50 });



            return Json(recentlyPlayed);
        }
        #endregion






        #region Spotify API Access
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
        public IActionResult PlaybackState()
        {
            SetupApi(out var username);

            var playbackState = _spotify.Get(new GetPlaybackState());
            return Json(playbackState);
        }

        public IActionResult TrackView(string trackId)
        {
            SetupApi();

            var track = _spotify.Get(new SpotifyGetTrack { TrackId = trackId });

            JakeLoadItemIntoDatabase(track);

            using (var db = _factory.OpenDbConnection())
            {
                var itemIds = new List<string>() { track.Id, track.Album.Id };
                itemIds.AddRange(track.Artists.Select(a => a.Id));
                itemIds.AddRange(track.Album.Artists.Select(a => a.Id));

                var query = db.From<GroupRelationship>()
                    .Join<Group>((gr, g) => gr.GroupId == g.GroupId)
                    .Where<GroupRelationship>(gr => Sql.In(gr.ItemId, itemIds));

                var results = db.Select<FullGroupRelationship>(query);
               
                return View(new TrackViewModel
                {
                    Track = track,
                    GridData = results
                });
            }
        }




        private void JakeLoadItemIntoDatabase(Track track)
        {
            var itemIds = new List<string>() { track.Id, track.Album.Id };
            itemIds.AddRange(track.Artists.Select(a => a.Id));
            itemIds.AddRange(track.Album.Artists.Select(a => a.Id));

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

            foreach (var artist in track.Artists.Concat(track.Album.Artists))
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
                db.InsertAll(toInsert);
            }
        }


        #endregion
    }
}
