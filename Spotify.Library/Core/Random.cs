using System;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    public abstract class SpotifyObject
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "uri")]
        public string Uri { get; set; }
    }

    public class SpotifyEntityReferenceObject
    {
        [DataMember(Name = "href")]
        public string Href { get; set; }

        [DataMember(Name = "total")]
        public int Total { get; set; }
    }

    public class ExternalUrls
    {
        [DataMember(Name = "spotify")]
        public string Spotify { get; set; }
    }

    public class SpotifyImage
    {
        [DataMember(Name = "height")]
        public int Height { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "width")]
        public int Width { get; set; }
    }

    public class ErrorObject
    {
        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "status")]
        public int Status { get; set; }
    }

    public class ErrorWrapper
    {
        [DataMember(Name = "error")]
        public ErrorObject Error { get; set; }
    }

    public class RawAccessTokenResponse
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }

        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }

        [DataMember(Name = "expires_in")]
        public double ExpiresIn { get; set; }
    }

    public class SpotifyToken
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime Expiration { get; set; }

        public bool IsExpired() => DateTime.Now >= Expiration;
    }
}
