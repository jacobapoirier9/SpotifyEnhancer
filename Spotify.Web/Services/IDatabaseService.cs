using Spotify.Web.Models;
using System.Collections.Generic;


namespace Spotify.Web.Services
{
    public interface IDatabaseService
    {
        List<FindGroupsResponse> FindGroups(FindGroups request, string username);
        FindGroupsResponse GetGroup(GetGroup request, string username);
        FindGroupsResponse SaveGroup(SaveGroup request, string username);

        List<FindItemsResponse> FindItems(FindItems request, string username);
    }
}
