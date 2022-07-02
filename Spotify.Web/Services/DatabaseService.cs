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

        //private bool RequireAdmin(string username) => IsAdminUser(username) ? true : throw new Exception($"User {username} is not authorized for this operation");

        //private bool IsAdminUser(string username)
        //{
        //    var adminUsers = new List<string>() { "jacobapoirier9" };
        //    var isAdminUser = adminUsers.Contains(username);
        //    return isAdminUser;
        //}
        

        public List<FindGroupsResponse> FindGroups(FindGroups request, string username)
        {
            using (var db = _factory.Open())
            {
                var query = db.From<FindGroupsResponse>()
                    .Where(g => g.Username == username);

                return db.Select(query);
            }
        }

        public FindGroupsResponse GetGroup(GetGroup request, string username)
        {
            using (var db = _factory.Open())
            {
                var query = db.From<FindGroupsResponse>()
                    .Where(g => g.Username == username);

                return db.Single(query);
            }
        }
        public FindGroupsResponse SaveGroup(SaveGroup request, string username)
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

        public List<FindRelationshipsResponse> FindGroupRelationships(FindGroupRelationships request, string username)
        {
            using (var db = _factory.Open())
            {
                var query = db.From<FindRelationshipsResponse>()
                    .Where(r => r.Username == username);

                if (request.ItemIds is not null && request.ItemIds.Count > 0)
                {
                    query.Where(r => Sql.In(r.ItemId, request.ItemIds));
                }

                return db.Select(query);
            }
        }
    }
}
