using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotify.Library
{
    public static class SpotifyUris
    {
        public static string LoginUrl(IConfiguration configuration) => LoginUrl(configuration.Get<string>("Spotify:RedirectUri"), configuration.Get<string>("Spotify:ClientId"));
        public static string LoginUrl(string redirectUri, string clientId)
        {
            var response = "https://accounts.spotify.com/authorize".BuildUri(new
            {
                response_type = "code",
                redirect_uri = redirectUri,
                client_id = clientId,
                scope = new string[]
                    {
                        "playlist-read-private", "user-read-playback-position", "user-read-email", "user-library-modify",
                        "playlist-read-collaborative", "playlist-modify-private", "user-follow-read", "user-read-playback-state",
                        "user-read-currently-playing", "user-read-recently-played", "user-modify-playback-state", "ugc-image-upload",
                        "playlist-modify-public", "user-top-read", "user-library-read", "user-read-private", "user-follow-modify"
                    }.JoinToString('+')
            });

            return response;
        }
    }
}
