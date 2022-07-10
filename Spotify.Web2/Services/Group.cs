﻿using ServiceStack.DataAnnotations;
using Spotify.Library.Core;
using System.Collections.Generic;

namespace Spotify.Web.Services
{
    public class FindGroups
    {
        public List<string> ItemIds { get; set; }
    }

    public class GetGroup
    {
        public int GroupId { get; set; }
    }

    public class FindItems
    {
        public int? GroupId { get; set; }
        public ItemType? ItemType { get; set; }
        //public List<string> ItemIds { get; set; }
    }

    public class SaveGroup
    {
        public string GroupName { get; set; }
    }

    public class FullGroup
    {
        public string Username { get; set; }

        public int GroupId { get; set; }

        public string GroupName { get; set; }

        public int TrackCount { get; set; }

        public int AlbumCount { get; set; }

        public int ArtistCount { get; set; }
    }

    public class FullItem
    {
        public string ItemId { get; set; }

        public string Name { get; set; }

        public ItemType ItemType { get; set; }

    }

    //[Schema("Spotify")]
    //[Alias("FindRelationships")]
    //public class FullItem
    //{
    //    public string Username { get; set; }

    //    public string RelationshipId { get; set; }

    //    public string GroupId { get; set; }

    //    public string GroupName { get; set; }

    //    public ItemType ItemType { get; set; }

    //    public string ItemId { get; set; }
    //}
}
