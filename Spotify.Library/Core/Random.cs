using System;
using System.Runtime.Serialization;

namespace Spotify.Library.Core
{
    public abstract class SpotifyObject
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public ItemType Type { get; set; }

        public string Uri { get; set; }
    }

    public class SpotifyEntityReferenceObject
    {
        public string Href { get; set; }

        public int Total { get; set; }
    }

    public class ExternalUrls
    {
        public string Spotify { get; set; }
    }

    public class SpotifyImage
    {
        public int Height { get; set; }

        public string Url { get; set; }

        public int Width { get; set; }
    }

    public class ErrorObject
    {
        public string Message { get; set; }

        public int Status { get; set; }
    }

    public class ErrorWrapper
    {
        public ErrorObject Error { get; set; }
    }

    public class RawAccessTokenResponse
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string TokenType { get; set; }

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
