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

        private void SetupApi() => SetupApi(out _);
        private void SetupApi(out string username)
        {
            username = _username;
            var token = this.Claim<string>(Names.AccessToken);

            _spotify.BearerToken = token;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View("Index");
        }
        
       






        [HttpGet]
        public IActionResult Groups(int? groupId)
        {
            if (groupId.HasValue)
            {
                var group = _service.GetGroup(new GetGroup { GroupId = groupId.Value }, _username);
                return View("GroupSingle", _service.GetGroup(new GetGroup { GroupId = groupId.Value }, _username));
            }
            else
            {
                return View("GroupsMultiple");
            }
        }

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

        [HttpGet]
        public IActionResult Track(string trackId)
        {
            SetupApi(out var username);

            var track = _spotify.Get(new GetTrack { TrackId = trackId });
            _cache.Save(username, track);

            return View("TrackSingle", new TrackSingleViewModel
            {
                Track = track
            });
        }


        [HttpGet]
        [HttpPost]
        public IActionResult RecentlyPlayed() // While not used yet, this will be used to create bulk playlists / groupings on your listen history
        {
            SetupApi(out var username);
            var recentlyPlayed = _spotify.Get(new GetRecentlyPlayedTracks() { Limit = 50 });
            return Json(recentlyPlayed);
        }

        [Ajax]
        [HttpPost]
        public IActionResult GetCurrentlyPlaying() // Possibly implement caching of the response to add "add currently playing to ___" feature
        {
            SetupApi(out var username);

            var playbackState = _spotify.Get(new GetPlaybackState());
            //var currentlyPlaying = _spotify.Get(new GetCurrentlyPlaying());

            return Json(playbackState);
        }
        
        [Ajax]
        public IActionResult GetGroupsForCurrentTrack()
        {
            var track = _cache.Get<Track>(_username);

            var itemIds = new List<string>() { track.Id, track.Album.Id };
            itemIds.AddRange(track.Artists.Select(a => a.Id));
            itemIds.AddRange(track.Album.Artists.Select(a => a.Id));

            var groups = _service.FindGroups(new FindGroups { ItemIds = itemIds }, _username);
            return Json(groups);
        }

        [Ajax]
        public IActionResult GetItemsForGroup(int groupId)
        {
            var track = _cache.Get<Track>(_username);

            var itemIds = new List<string>() { track.Id, track.Album.Id };
            itemIds.AddRange(track.Artists.Select(a => a.Id));
            itemIds.AddRange(track.Album.Artists.Select(a => a.Id));

            var relatedItems = _service.FindItems(new FindItems { GroupId = groupId, ItemIds = itemIds }, _username);

            var trackIds = relatedItems.Where(ri => ri.ItemType == ItemType.Track).Select(t => t.ItemId).ToList();
            var albumIds = relatedItems.Where(ri => ri.ItemType == ItemType.Album).Select(t => t.ItemId).ToList();
            var artistIds = relatedItems.Where(ri => ri.ItemType == ItemType.Artist).Select(t => t.ItemId).ToList();

            var tracks = trackIds.Count > 0 ? _spotify.Get(new GetTracks { Ids = trackIds }).Tracks : new List<Track>();
            var albums = albumIds.Count > 0 ? _spotify.Get(new GetAlbums { Ids = albumIds }).Albums : new List<Album>();
            var artists = artistIds.Count > 0 ? _spotify.Get(new GetArtists { Ids = artistIds }).Artists : new List<Artist>();

            var spotifyItems = new List<SpotifyObject>();
            spotifyItems.AddRange(tracks);
            spotifyItems.AddRange(albums);
            spotifyItems.AddRange(artists);

            return Json(spotifyItems);
        }
    }
}
