using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spotify.Web.Controllers
{
    public class AjaxAttribute : Attribute { }

    [Authorize]
    public class SpotifyController : Controller
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceClient _spotify;
        private readonly ICustomCache _cache;
        private IDatabaseService _service;

        private string _username => this.Claim<string>(Names.Username);

        public SpotifyController(IServiceClient spotify, IDatabaseService service, ICustomCache cache)
        {
            _spotify = spotify;
            _cache = cache;
            _service = service;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View("PlaylistBuilder");
        }
        
        [HttpGet]
        [HttpPost]
        public IActionResult RecentlyPlayed()
        {
            SetupApi(out var username);
            var recentlyPlayed = _spotify.Get(new GetRecentlyPlayedTracks() { Limit = 50 });
            return Json(recentlyPlayed);
        }






        [HttpGet]
        public IActionResult Groups() => View();

        [Ajax]
        [HttpPost]
        public IActionResult GetGroups()
        {
            var groups = _service.FindGroups(new FindGroups(), _username);
            return Json(groups);
        }

        [Ajax]
        [HttpPost]
        public IActionResult SaveGroup(SaveGroup toSave)
        {
            var group = _service.SaveGroup(toSave, _username);
            return Json(group);
        }




        private void SetupApi() => SetupApi(out _);
        private void SetupApi(out string username)
        {
            username = _username;
            var token = this.Claim<string>(Names.AccessToken);

            _spotify.BearerToken = token;

            _logger.Trace("{Username} is using the API with token {Token}", username, token);
        }

        //[Obsolete("Remove the Categories action method")]
        //[HttpGet]
        //public IActionResult Categories()
        //{
        //    SetupApi();

        //    var categories = _spotify.GetAll(new GetCategories(), response => response.Categories)
        //        .OrderBy(c => c.Name);

        //    var grid = Helpers.ToArrayGrid(categories, 4);
        //    return View(grid);
        //}


        [Ajax]
        [HttpPost]
        public IActionResult GetCurrentlyPlaying()
        {
            SetupApi(out var username);

            var playbackState = _spotify.Get(new GetPlaybackState());

            // This might also return the url to a playlist in the context object, if it's playing form a playlist.
            // This can be used to "Add to currently playing playlist" feature.
            //var currentlyPlaying = _spotify.Get(new GetCurrentlyPlaying());

            return Json(playbackState);
        }

        [HttpGet]
        public IActionResult Track(string trackId)
        {
            SetupApi(out var username);

            var track = _spotify.Get(new GetTrack { TrackId = trackId });
            _cache.Save(username, track);

            return View("TrackView", new TrackViewModel
            {
                Track = track
            });
        }

        [Ajax]
        [HttpPost]
        public IActionResult GetGroupsForTrack()
        {
            var track = _cache.Get<Track>(this.Claim<string>(Names.Username));

            var itemIds = new List<string>() { track.Id, track.Album.Id };
            itemIds.AddRange(track.Artists.Select(a => a.Id));
            itemIds.AddRange(track.Album.Artists.Select(a => a.Id));

            var relationships = _service.FindGroupRelationships(new FindGroupRelationships { ItemIds = itemIds }, _username);

            return Json(relationships.Select(r => new
            {
                r.GroupId,
                r.GroupName,
                r.ItemType,
                r.ItemId,
                AddedTo = r.ItemType switch
                {
                    "track" => track.Name,
                    "album" => track.Album.Name,
                    "artist" => track.AllUniqueArtists.Select(artist => artist.Name).Join(","),

                    _ => throw new IndexOutOfRangeException(nameof(r.ItemType))
                } + $" ({r.ItemType})"
            }));
        }
    }
}
