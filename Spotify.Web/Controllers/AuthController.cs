using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NLog;
using ServiceStack;
using ServiceStack.Data;
using Spotify.Library;
using Spotify.Library.Core;
using Spotify.Library.Services;
using Spotify.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Spotify.Web.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;
        private readonly ISpotifyTokenService _tokens;
        private readonly IServiceClient _spotify;

        public AuthController(IConfiguration configuration, ISpotifyTokenService tokens, IServiceClient spotify)
        {
            _configuration = configuration;
            _tokens = tokens;
            _spotify = spotify;
        }

        public IActionResult Login(string code)
        {
            if (code is null)
            {
                var response = SpotifyUris.LoginUrl(_configuration);
                return Redirect(response);
            }
            else
            {
                var token = _tokens.CodeForAccessToken(code);
                var user = _spotify.Get(new SpotifyGetCurrentlySignedInUser(), token.AccessToken);

                var claims = new List<Claim>();
                claims.AddClaim("SpotifyToken", token);
                claims.AddClaim("SpotifyUser", user);

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                HttpContext.SignInAsync(claimsPrincipal);

                if (_configuration.Get<bool>("DeepLogging"))
                {
                    var builder = new StringBuilder();
                    builder.AppendLine("Registering claims..");
                    claims.Each(claim => builder.AppendLine($"Type: {claim.Type}, Value: {claim.Value}"));

                    _logger.Trace(builder.ToString());
                }

                return RedirectToAction("Index", "Spotify");
            }
        }
    }
}
