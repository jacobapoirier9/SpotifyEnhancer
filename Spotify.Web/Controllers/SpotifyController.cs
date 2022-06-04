using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using ServiceStack;
using Spotify.Library.Core;
using Spotify.Library.Services;
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
        public SpotifyController(IServiceClient spotify)
        {
            _spotify = spotify;
        }

        private void SetupApi()
        {
            var user = this.Claim<SpotifyUser>();
            var token = this.Claim<SpotifyToken>();

            _spotify.BearerToken = token.AccessToken;

            _logger.Debug("About to use Spotify API for user {Username}", user.DisplayName);
        }

        [HttpGet]
        public IActionResult Index()
        {
            SetupApi();

            var categories = _spotify.GetAll(new SpotifyFindCategories(), response => response.Categories);

            var grid = Helpers.ToArrayGrid(categories, 4);

            return View(grid);
        }
    }
}
