using Microsoft.Extensions.Configuration;
using NLog;
using ServiceStack;
using Spotify.Library.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Spotify.Library.Services
{
        public interface ISpotifyTokenService
        {
            SpotifyToken CodeForAccessToken(string code);

            SpotifyToken ValidateAccessToken(SpotifyToken token);
        }


        public class SpotifyTokenService : ISpotifyTokenService
        {
            private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
            private IConfiguration _configuration;

            public SpotifyTokenService(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public SpotifyToken CodeForAccessToken(string code)
            {
                var clientId = _configuration.Get<string>("Spotify:ClientId");
                var clientSecret = _configuration.Get<string>("Spotify:ClientSecret");
                var redirectUri = _configuration.Get<string>("Spotify:RedirectUri");
                var tokenUri = _configuration.Get<string>("Spotify:TokenUri");

                var requestBody = new
                {
                    grant_type = "authorization_code",
                    code = code,
                    redirect_uri = redirectUri
                };

                var request = WebRequest.Create(tokenUri.BuildUri(requestBody));
                request.Method = "POST";
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}")));

                request.ContentLength = string.Empty.Length;
                request.ContentType = "application/x-www-form-urlencoded";

                try
                {
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        var rawToken = json.FromJson<RawAccessTokenResponse>();

                        var token = new SpotifyToken
                        {
                            AccessToken = rawToken.AccessToken,
                            RefreshToken = rawToken.RefreshToken,
                            Expiration = DateTime.Now.AddSeconds(rawToken.ExpiresIn)
                        };

                        _logger.Trace("Token exchange was successful. New token: {Token}", rawToken.AccessToken);

                        return token;
                    }
                }
                catch (WebException ex)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        var error = json.FromJson<ErrorWrapper>().Error;

                        _logger.Error("Failed to get a spotify token. Spotify Error: {Error}", error?.Message ?? "None");

                        throw new WebException(ex.Message);
                    }
                }
            }

            public SpotifyToken ValidateAccessToken(SpotifyToken token)
            {
                if (token.IsExpired())
                {
                    var clientId = _configuration.Get<string>("Spotify:ClientId");
                    var clientSecret = _configuration.Get<string>("Spotify:ClientSecret");
                    var tokenUri = _configuration.Get<string>("Spotify:TokenUri");

                    var requestObj = new
                    {
                        grant_type = "refresh_token",
                        refresh_token = token.RefreshToken
                    };

                    var request = WebRequest.Create(tokenUri);
                    request.Method = "POST";
                    request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}")));

                    var body = string.Empty.BuildUri(requestObj);

                    request.ContentLength = body.Length;
                    request.ContentType = "application/x-www-form-urlencoded";

                    using (var stream = request.GetRequestStream())
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(body);
                    }

                    try
                    {
                        using (var response = request.GetResponse())
                        using (var stream = response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            var json = reader.ReadToEnd();
                            var rawToken = json.FromJson<RawAccessTokenResponse>();

                            token = new SpotifyToken
                            {
                                AccessToken = rawToken.AccessToken,
                                RefreshToken = rawToken.RefreshToken,
                                Expiration = DateTime.Now.AddSeconds(rawToken.ExpiresIn)
                            };

                            _logger.Trace("Refresh for token exchange was successful. New token: {Token}", token.AccessToken);

                            return token;
                        }
                    }
                    catch (WebException ex)
                    {
                        using (var stream = ex.Response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            var json = reader.ReadToEnd();
                            var error = json.FromJson<ErrorWrapper>().Error;

                            _logger.Error("Failed to get a spotify token. Spotify Error: {Error}", error.Message ?? "None");

                            throw new WebException(ex.Message);
                        }
                    }
                }
                else
                {
                    return token;
                }
            }
        }
}
