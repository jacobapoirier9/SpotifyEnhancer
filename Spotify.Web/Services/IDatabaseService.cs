using Spotify.Web.Database;
using Spotify.Web.Models;
using System.Collections.Generic;


namespace Spotify.Web.Services
{
    public interface IDatabaseService
    {
        List<FullGroup> FindGroups(FindGroups request);
        FullGroup GetGroup(GetGroup request);
        FullGroup SaveGroup(SaveGroup request);

        List<FullGroupRelationship> FindGroupRelationships(FindGroupRelationships request);
    }
}
