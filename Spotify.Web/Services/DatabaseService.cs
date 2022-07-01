using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spotify.Web.Database;
using Spotify.Web.Models;
using System;
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

        private bool RequireAdmin(string username) => IsAdminUser(username) ? true : throw new Exception($"User {username} is not authorized for this operation");

        private bool IsAdminUser(string username)
        {
            var adminUsers = new List<string>() { "jacobapoirier9" };
            var isAdminUser = adminUsers.Contains(username);
            return isAdminUser;
        }
        




        public List<FullGroup> FindGroups(FindGroups request, string username)
        {
            using (var db = _factory.Open())
            {
                var query = FindGroupsExpression(db, username);
                return db.Select<FullGroup>(query);
            }
        }

        public FullGroup GetGroup(GetGroup request, string username)
        {
            using (var db = _factory.Open())
            {
                var query = FindGroupsExpression(db, username)
                    .Where(g => g.GroupId == request.GroupId);

                return db.Single<FullGroup>(query);
            }
        }
        public FullGroup SaveGroup(SaveGroup request, string username)
        {
            using (var db = _factory.Open())
            {
                var id = (int)db.Insert(new DbGroup
                {
                    Username = username,
                    GroupName = request.GroupName,
                    GroupDescription = request.GroupDescription
                }, true);

                return GetGroup(new GetGroup { GroupId = id }, username);
            }
        }

        public List<FullGroupRelationship> FindGroupRelationships(FindGroupRelationships request, string username)
        {
            using (var db = _factory.Open())
            {
                if (request.ItemIds is not null && request.ItemIds.Count > 0)
                {
                    var query = db.From<DbGroupRelationship>()
                        .Join<DbGroup>((gr, g) => gr.GroupId == g.GroupId)
                        .Where<DbGroupRelationship>(gr => Sql.In(gr.ItemId, request.ItemIds) && gr.Username == username);

                    var results = db.Select<FullGroupRelationship>(query);

                    return results;
                }

                return default;
            }
        }
    }
}
