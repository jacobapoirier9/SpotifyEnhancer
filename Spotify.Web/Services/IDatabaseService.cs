using Spotify.Web.Database;
using Spotify.Web.Models;
using System.Collections.Generic;


namespace Spotify.Web.Services
{
    public interface IDatabaseService
    {
        List<FullGroup> FindGroups(FindGroups request, string username);
        FullGroup GetGroup(GetGroup request, string username);
        FullGroup SaveGroup(SaveGroup request, string username);

        List<FullGroupRelationship> FindGroupRelationships(FindGroupRelationships request, string username);
    }
}
