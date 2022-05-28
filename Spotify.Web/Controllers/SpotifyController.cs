using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ISpotifyTokenService _tokens;
        private readonly IServiceClient _spotify;
        public SpotifyController(ISpotifyTokenService tokens, IServiceClient spotify)
        {
            _tokens = tokens;
            _spotify = spotify;
        }

        public void SetupApi()
        {
            var token = this.Claim<SpotifyToken>();
            _spotify.BearerToken = token.AccessToken;
        }


        public IActionResult Index()
        {
            SetupApi();
            var user = this.Claim<SpotifyUser>();

            var categories = _spotify.GetAll(new SpotifyFindCategories(), response => response.Categories);

            return View();
        }





    }
}
