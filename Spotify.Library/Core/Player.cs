using ServiceStack;
using ServiceStack.DataAnnotations;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    [Route("/me/player")]
    public class SpotifyGetPlaybackState : IReturn<SpotifyPlaybackState>, IGet
    {

    }

    public class SpotifyPlaybackDevice
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "volume_percent")]
        public byte VolumePercent { get; set; }
    }

    public class SpotifyPlaybackState
    {
        [DataMember(Name = "device")]
        public SpotifyPlaybackDevice Device { get; set; }

        [DataMember(Name = "shuffle_state")]
        public bool ShuffleState { get; set; }

        [DataMember(Name = "repeat_state")]
        public bool RepeatState { get; set; }

        [DataMember(Name = "timestamp")]
        public long TimeStamp { get; set; }



        [DataMember(Name = "item")]
        public SpotifyTrack Item { get; set; }

        [DataMember(Name = "is_playing")]
        public bool IsPlaying { get; set; }

        [DataMember(Name = "progress_ms")]
        public long ProgressMs { get; set; }
    }

    /*
     {
  "progress_ms": 153264,
  "item": {
    "album": {
      "album_type": "single",
      "artists": [
        {
          "external_urls": {
            "spotify": "https://open.spotify.com/artist/0pCNk4D3E2xtszsm6hMsWr"
          },
          "href": "https://api.spotify.com/v1/artists/0pCNk4D3E2xtszsm6hMsWr",
          "id": "0pCNk4D3E2xtszsm6hMsWr",
          "name": "K.Flay",
          "type": "artist",
          "uri": "spotify:artist:0pCNk4D3E2xtszsm6hMsWr"
        }
      ],
      "external_urls": {
        "spotify": "https://open.spotify.com/album/3GkyRKyYzpB4v0fjFBkUdr"
      },
      "href": "https://api.spotify.com/v1/albums/3GkyRKyYzpB4v0fjFBkUdr",
      "id": "3GkyRKyYzpB4v0fjFBkUdr",
      "images": [
        {
          "height": 640,
          "url": "https://i.scdn.co/image/ab67616d0000b273402ea363d05cf3af7db2ef45",
          "width": 640
        },
        {
          "height": 300,
          "url": "https://i.scdn.co/image/ab67616d00001e02402ea363d05cf3af7db2ef45",
          "width": 300
        },
        {
          "height": 64,
          "url": "https://i.scdn.co/image/ab67616d00004851402ea363d05cf3af7db2ef45",
          "width": 64
        }
      ],
      "name": "High Enough (RAC Remix)",
      "release_date": "2017-08-04",
      "release_date_precision": "day",
      "total_tracks": 1,
      "type": "album",
      "uri": "spotify:album:3GkyRKyYzpB4v0fjFBkUdr"
    },
    "artists": [
      {
        "external_urls": {
          "spotify": "https://open.spotify.com/artist/0pCNk4D3E2xtszsm6hMsWr"
        },
        "href": "https://api.spotify.com/v1/artists/0pCNk4D3E2xtszsm6hMsWr",
        "id": "0pCNk4D3E2xtszsm6hMsWr",
        "name": "K.Flay",
        "type": "artist",
        "uri": "spotify:artist:0pCNk4D3E2xtszsm6hMsWr"
      },
      {
        "external_urls": {
          "spotify": "https://open.spotify.com/artist/4AGwPDdh1y8hochNzHy5HC"
        },
        "href": "https://api.spotify.com/v1/artists/4AGwPDdh1y8hochNzHy5HC",
        "id": "4AGwPDdh1y8hochNzHy5HC",
        "name": "RAC",
        "type": "artist",
        "uri": "spotify:artist:4AGwPDdh1y8hochNzHy5HC"
      }
    ],
    "disc_number": 1,
    "duration_ms": 208866,
    "explicit": false,
    "external_ids": {
      "isrc": "USUM71708345"
    },
    "external_urls": {
      "spotify": "https://open.spotify.com/track/1cxttvflVlYD4QA34tR5Yj"
    },
    "href": "https://api.spotify.com/v1/tracks/1cxttvflVlYD4QA34tR5Yj",
    "id": "1cxttvflVlYD4QA34tR5Yj",
    "is_local": false,
    "name": "High Enough - RAC Remix",
    "popularity": 70,
    "preview_url": "https://p.scdn.co/mp3-preview/166483f50fcc1c0445871448e090bc5491539fa8?cid=774b29d4f13844c495f206cafdad9c86",
    "track_number": 1,
    "type": "track",
    "uri": "spotify:track:1cxttvflVlYD4QA34tR5Yj"
  },
  "currently_playing_type": "track",
  "actions": {
    "disallows": {
      "resuming": true
    }
  },
  "is_playing": true
}
     */
}
