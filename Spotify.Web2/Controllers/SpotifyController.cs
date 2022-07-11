using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using Spotify.Library.Core;
using Spotify.Library.Services;
using Spotify.Web.Models;
using Spotify.Web.Models.Database;
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
        private IDbConnectionFactory _database;

        private string _username => this.Claim<string>(Names.Username);

        public SpotifyController(IServiceClient spotify, ICustomCache cache, IDbConnectionFactory database)
        {
            _spotify = spotify;
            _cache = cache;
            _database = database;
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


        public IActionResult Groups(int? groupId)
        {
            using (var db = _database.Open())
            {
                var query = db.From<DbGroup2>()
                    .Where<DbGroup2>(g => g.Username == _username);

                if (groupId.HasValue)
                {
                    query.Where<DbGroup2>(g => g.GroupId == groupId.Value);
                    var group = db.Single<Group>(query);

                    var subQuery = db.From<DbGroup2>()
                        .Join<DbRelationship>((g, r) => g.GroupId == r.GroupId)
                        .Where(g => g.Username == _username && g.GroupId == groupId.Value)
                        .Select<DbRelationship>(r => r.TrackId);

                    var tracks = new List<Track>();
                    db.Select<string>(subQuery).ExecuteInChunks(50, dataSet =>
                    {
                        tracks.AddRange(_spotify.Get(new GetTracks { Ids = dataSet }).Tracks);
                    });

                    _cache.Save(_username, nameof(CachedTracks), tracks);

                    return View("GroupSingle", group);
                }
                else
                {
                    var groups = db.Select<Group>(query);
                    _cache.Save(_username, nameof(CachedGroups), groups);

                    return View("GroupsMultiple");
                }
            }
        }

        public IActionResult CachedGroups()
        {
            var groups = _cache.Get<List<Group>>(_username, nameof(CachedGroups));
            return Json(groups);
        }

        public IActionResult CachedTracks()
        {
            var tracks = _cache.Get<List<Track>>(_username, nameof(CachedTracks));
            return Json(tracks);
        }

        public IActionResult CachedRecommendations()
        {
            var tracks = _cache.Get<List<Track>>(_username, nameof(CachedRecommendations));
            return Json(tracks);
        }

        //[Ajax]
        //[HttpPost]
        //public IActionResult SaveGroup(Group save)
        //{
        //    using (var db = _database.Open())
        //    {
        //        var id = db.Insert(new DbGroup2
        //        {
        //            Username = _username,
        //            GroupName = save.GroupName
        //        }, true);

        //        var query = db.From<DbGroup2>()
        //            .Where(g => g.Username == _username && g.GroupId == id);

        //        var group = db.Single<Group>(query);
        //        return Json(group);
        //    }
        //}

        [HttpGet]
        public IActionResult Track(string trackId)
        {
            SetupApi(out var username);

            var track = _spotify.Get(new GetTrack { TrackId = trackId });
            _cache.Save(username, track);

            //var audioFeatures = _spotify.Get(new GetAudioFeatures { Id = trackId });
            //_cache.Save(username, audioFeatures);


            using (var db = _database.Open())
            {
                var query = db.From<DbGroup2>()
                    .Where<DbGroup2>(g => g.Username == _username)
                    .LeftJoin<DbRelationship>((g, r) => g.GroupId == r.GroupId && r.TrackId == trackId)
                    .Select<DbGroup2, DbRelationship>((g, r) => new
                    {
                        g.GroupId,
                        g.GroupName,
                        g.Username,
                        IsMember = Sql.Custom($"iif({db.GetDialectProvider().GetQuotedTableName(typeof(DbRelationship).GetModelMetadata())}.{nameof(DbRelationship.TrackId)}='{trackId}', 1, 0)")
                    });

                var groups = db.Select<Group>(query);
                _cache.Save(_username, nameof(CachedGroups), groups);

                if (_cache.IsExpired(username, nameof(CachedRecommendations)))
                {
                    var recommendations = _spotify.Get(new GetRecommendations
                    {
                        seed_tracks = new List<string>() { track.Id },
                        seed_artists = track.AllUniqueArtists.Select(a => a.Id).ToList()
                    }).Tracks;

                    _cache.Save(_username, nameof(CachedRecommendations), recommendations);
                }

                return View("TrackSingle", new TrackSingleViewModel
                {
                    Track = track,
                    IsLiked = _spotify.Get(new GetTrackIsLiked { Ids = trackId.AsList() }).First()
                });
            }
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
    }
}