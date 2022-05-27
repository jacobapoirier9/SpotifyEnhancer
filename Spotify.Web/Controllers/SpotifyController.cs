using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotify.Library.Core;
using Spotify.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spotify.Web.Controllers
{
    [Authorize]
    public class SpotifyController : Controller
    {
        private readonly ISpotifyTokenService _tokens;
        public SpotifyController(ISpotifyTokenService tokens)
        {
            _tokens = tokens;
        }


        public IActionResult Index()
        {
            var user = this.Claim<SpotifyUser>();
            return View();
        }
    }
}
