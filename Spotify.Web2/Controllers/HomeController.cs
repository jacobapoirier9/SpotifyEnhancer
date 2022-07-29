using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using Spotify.Library.Core;
using Spotify.Web.Services;
using System;
using System.Collections.Generic;

namespace Spotify.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IServiceClient _spotify;
        private readonly ICustomCache _cache;
        public HomeController(IServiceClient spotify, ICustomCache cache)
        {
            _spotify = spotify;
            _cache = cache;
        }


        private void SetupApi() => SetupApi(out _);
        private void SetupApi(out string username)
        {
            username = this.Claim<string>(Names.Username);
            var token = this.Claim<string>(Names.AccessToken);

            _spotify.BearerToken = token;
        }

        public IActionResult Index() => PlaylistMultiple();

        public IActionResult PlaylistMultiple()
        {
            SetupApi();

            var playlists = _spotify.GetAll(new GetPlaylists { Limit = 50 }, response => response);
            return View(playlists);
        }

        public IActionResult PlaylistSingle(string playlistId)
        {
            SetupApi();

            var tracks = _spotify.GetAll(new GetPlaylistItems { PlaylistId = playlistId }, response => response);
            return Json(tracks);
        }
    }
}
