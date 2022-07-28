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

        public IActionResult Index()
        {
            SetupApi();

            var playlists = _cache.Get(this.Claim<string>(Names.Username), "Playlists", () =>
            {
                return _spotify.GetAll(new GetPlaylists(), response => response);
            });
            //var playlists = _spotify.GetAll(new GetPlaylists(), response => response);
            return View(playlists);
        }
    }

    public static class CacheExtensions
    {
        public static T Get<T>(this ICustomCache cache, string username, string key, Func<T> func)
        {
            if (cache.HasKey(username, key))
            {
                return cache.Get<T>(username, key);
            }
            else
            {
                var response = func.Invoke();
                cache.Save(username, key, response);
                return response;
            }
        }
    }
}
