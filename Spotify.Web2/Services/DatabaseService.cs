using ServiceStack.Data;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using Spotify.Library.Core;
using Spotify.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace Spotify.Web.Services
{
    public class DatabaseService : IDatabaseService
    {
        [Schema("Spotify")]
        [Alias("GroupRelationship")]
        private class DbGroupRelationship
        {
            public string Username { get; set; }

            public int GroupId { get; set; }

            public ItemType ItemType { get; set; }

            public string ItemId { get; set; }
        }

        [Schema("Spotify")]
        [Alias("Groups")]
        private class DbGroup
        {
            [IgnoreOnInsert]
            public int GroupId { get; set; }

            public string Username { get; set; }

            public string GroupName { get; set; }

            public string GroupDescription { get; set; }
        }

        [Schema("Spotify")]
        [Alias("GroupsWithCounts")]
        private class DbGroupsWithCounts
        {
            public string Username { get; set; }

            public int GroupId { get; set; }

            public string GroupName { get; set; }

            public int TrackCount { get; set; }

            public int AlbumCount { get; set; }

            public int ArtistCount { get; set; }
        }


        private IDbConnectionFactory _factory;
        public DatabaseService(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        //private bool RequireAdmin(string username) => IsAdminUser(username) ? true : throw new Exception($"User {username} is not authorized for this operation");

        //private bool IsAdminUser(string username)
        //{
        //    var adminUsers = new List<string>() { "jacobapoirier9" };
        //    var isAdminUser = adminUsers.Contains(username);
        //    return isAdminUser;
        //}
        

        public List<FullGroup> FindGroups(FindGroups request, string username)
        {
            using (var db = _factory.Open())
            {
                var query = db.From<DbGroupsWithCounts>()
                    .Where(g => g.Username == username);
                
                if (request.ItemIds is not null && request.ItemIds.Count > 0)
                {
                    var subQuery = db.From<DbGroupRelationship>()
                        .Where(gr => Sql.In(gr.ItemId, request.ItemIds))
                        .Select(gr => gr.GroupId);

                    query.Where(g => Sql.In(g.GroupId, subQuery));
                }

                var groups = db.Select(query);
                return ToDto(groups);
            }
        }

        public FullGroup GetGroup(GetGroup request, string username)
        {
            using (var db = _factory.Open())
            {
                var query = db.From<DbGroupsWithCounts>()
                    .Where(g => g.Username == username && g.GroupId == request.GroupId);

                var groupWithCounts = db.Single(query);
                return ToDto(groupWithCounts);
            }
        }
        public FullGroup SaveGroup(SaveGroup request, string username)
        {
            using (var db = _factory.Open())
            {
                var id = (int)db.Insert(new DbGroup
                {
                    Username = username,
                    GroupName = request.GroupName
                }, true);

                return GetGroup(new GetGroup { GroupId = id }, username);
            }
        }

        public List<FullItem> FindItems(FindItems request, string username)
        {
            using (var db = _factory.Open())
            {
                var query = db.From<DbGroupRelationship>()
                    .Where(gr => gr.Username == username);

                if (request.GroupId.HasValue)
                {
                    query.Where(gr => gr.GroupId == request.GroupId.Value);
                }
                
                if (request.ItemType.HasValue)
                {
                    query.Where(gr => gr.ItemType == request.ItemType.Value);
                }

                var relationships = db.Select(query);
                return ToDto(relationships);
            }
        }

        //public List<FullItem> FindItems(FindItems request, string username)
        //{
        //    using (var db = _factory.Open())
        //    {
        //        var query = db.From<DbGroupRelationship>()
        //            .Join<DbGroup>((gr, g) => gr.GroupId == g.GroupId)
        //            .Where(gr => gr.Username == username)
        //            .SelectDistinct();

        //        if (request.GroupId.HasValue)
        //        {
        //            query.Where(gr => gr.GroupId == request.GroupId);
        //        }
        //        if (request.ItemIds is not null && request.ItemIds.Count > 0)
        //        {
        //            query.Where(gr => Sql.In(gr.ItemId, request.ItemIds));
        //        }

        //        var items = db.Select<FullItem>(query);
        //        return items;
        //    }
        //}

        private static List<FullGroup> ToDto(List<DbGroupsWithCounts> from) => from.ConvertAll(ToDto);
        private static FullGroup ToDto(DbGroupsWithCounts from)
        {
            var to = new FullGroup
            {
                GroupId = from.GroupId,
                GroupName = from.GroupName,
                Username = from.Username,
                ArtistCount = from.ArtistCount,
                AlbumCount = from.AlbumCount,
                TrackCount = from.TrackCount
            };

            return to;
        }

        private static List<FullItem> ToDto(List<DbGroupRelationship> from) => from.ConvertAll(ToDto);
        private static FullItem ToDto(DbGroupRelationship from)
        {
            var to = new FullItem
            {
                ItemId = from.ItemId,
                ItemType = from.ItemType
            };

            return to;
        }
    }
}
