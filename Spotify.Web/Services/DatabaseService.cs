using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spotify.Web.Database;
using Spotify.Web.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace Spotify.Web.Services
{
    public class DatabaseService : IDatabaseService
    {
        private IDbConnectionFactory _factory;
        public DatabaseService(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        private SqlExpression<DbGroup> FindGroupsExpression(IDbConnection db, string username)
        {
            var tableAlias = db.GetDialectProvider().GetQuotedTableName(typeof(DbGroupRelationship).GetModelMetadata());
            var query = db.From<DbGroup>()
                .LeftJoin<DbGroup, DbGroupRelationship>((g, gr) => g.Username == gr.Username && g.GroupId == gr.GroupId)
                .GroupBy<DbGroup>(g => new
                {
                    g.GroupId,
                    g.GroupName,
                    g.GroupDescription
                })
                .Where<DbGroup>(g => g.Username == username)
                .Select<DbGroup, DbGroupRelationship>((g, gr) => new
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    GroupDescription = g.GroupDescription,
                    TrackCount = Sql.Sum($"case when {tableAlias}.ItemType = 'track' then 1 else 0 end"),
                    AlbumCount = Sql.Sum($"case when {tableAlias}.ItemType = 'album' then 1 else 0 end"),
                    ArtistCount = Sql.Sum($"case when {tableAlias}.ItemType = 'artist' then 1 else 0 end")
                });

            return query;
        }


        public List<FullGroup> FindGroups(FindGroups request)
        {
            using (var db = _factory.Open())
            {
                var query = FindGroupsExpression(db, request.Username);
                return db.Select<FullGroup>(query);
            }
        }

        public FullGroup GetGroup(GetGroup request)
        {
            using (var db = _factory.Open())
            {
                var query = FindGroupsExpression(db, request.Username)
                    .Where(g => g.GroupId == request.GroupId);

                return db.Single<FullGroup>(query);
            }
        }
        public FullGroup SaveGroup(SaveGroup request)
        {
            using (var db = _factory.Open())
            {
                var id = (int)db.Insert(new DbGroup
                {
                    Username = request.Username,
                    GroupName = request.GroupName,
                    GroupDescription = request.GroupDescription
                }, true);

                return GetGroup(new GetGroup { Username = request.Username, GroupId = id });
            }
        }

        public List<FullGroupRelationship> FindGroupRelationships(FindGroupRelationships request)
        {
            using (var db = _factory.Open())
            {
                if (request.ItemIds is not null && request.ItemIds.Count > 0)
                {
                    var query = db.From<DbGroupRelationship>()
                        .Join<DbGroup>((gr, g) => gr.GroupId == g.GroupId)
                        .Where<DbGroupRelationship>(gr => Sql.In(gr.ItemId, request.ItemIds));

                    var results = db.Select<FullGroupRelationship>(query);

                    return results;
                }

                return default;
            }
        }


        /*
         * //var track = _cache.Get<Track>(this.Claim<string>(Names.Username));

            //using(var db = _factory.OpenDbConnection())
            //{
            //    var itemIds = new List<string>() { track.Id, track.Album.Id };
            //    itemIds.AddRange(track.Artists.Select(a => a.Id));
            //    itemIds.AddRange(track.Album.Artists.Select(a => a.Id));

            //    var query = db.From<DbGroupRelationship>()
            //        .Join<DbGroup>((gr, g) => gr.GroupId == g.GroupId)
            //        .Where<DbGroupRelationship>(gr => Sql.In(gr.ItemId, itemIds));

            //    var results = db.Select<FullGroupRelationship>(query);

            //    return Json(results.Select(r => new
            //    {
            //        r.GroupId,
            //        r.GroupName,
            //        r.ItemType,
            //        r.ItemId,
            //        AddedTo = r.ItemType switch
            //        {
            //            "track" => track.Name,
            //            "album" => track.Album.Name,
            //            "artist" => track.AllUniqueArtists.Select(artist => artist.Name).Join(","),

            //            _ => throw new IndexOutOfRangeException(nameof(r.ItemType))
            //        } + $" ({r.ItemType})"
            //    }));
            //}
         */
    }
}
