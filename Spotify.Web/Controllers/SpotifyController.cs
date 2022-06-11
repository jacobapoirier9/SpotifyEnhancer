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

        private void SetupApi()
        {
            var username = this.Claim<string>(Names.Username);
            var token = this.Claim<string>(Names.AccessToken);

            _spotify.BearerToken = token;

            _logger.Debug("About to use Spotify API for user {Username}", username);
        }

        [HttpGet]
        public IActionResult GetGroups()
        {
            using (var db = _factory.OpenDbConnection())
            {
                var username = this.Claim<string>(Names.Username);
                var query = db.From<Group>()
                    .Where(g => g.Username == username);

                var groups = db.Select(query);

                _logger.Debug("Returning {Count} groups for user {User}", groups.Count, username);

                return Json(groups);
            }
        }

        [HttpGet]
        public IActionResult Index()
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
            var playbackState = _spotify.Get(new SpotifyGetPlaybackState());
            return Json(playbackState);
        }
    }
}
