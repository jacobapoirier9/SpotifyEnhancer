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

            var categories = _spotify.GetAll(new SpotifyFindCategories(), response => response.Categories)
                .OrderBy(c => c.Name);

            var grid = Helpers.ToArrayGrid(categories, 4);
            return View(grid);
        }

        [HttpGet]
        public IActionResult PlaybackState()
        {
            SetupApi(out var username);

            var playbackState = _spotify.Get(new SpotifyGetPlaybackState());
            return Json(playbackState);
        }

        public IActionResult TrackView(string trackId)
        {
            return TrackView2(trackId);
            SetupApi(out var username);

            var track = _spotify.Get(new SpotifyGetTrack { TrackId = trackId });
            var groups = GetUserGroups(username);

            using (var db = _factory.OpenDbConnection())
            {
                var query = db.From<GroupRelationship>()
                    .Where(gr => gr.ItemType == "track" && gr.ItemId == trackId)
                    .Select(gr => gr.GroupId);

                var memberedGroupIds = db.Select<int>(query);

                return View(new TrackViewModel
                {
                    Track = track,
                    Groups = groups,
                    MemberedGroupIds = memberedGroupIds
                });
            }
        }

        public IActionResult TrackView2(string trackId)
        {
            SetupApi();
            var track = _spotify.Get(new SpotifyGetTrack { TrackId = trackId });
            return View("TrackView", track);
        }
        #endregion
    }
   
}
