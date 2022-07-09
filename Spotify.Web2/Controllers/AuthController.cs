using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NLog;
using ServiceStack;
using ServiceStack.Data;
using Spotify.Library;
using Spotify.Library.Core;
using Spotify.Library.Services;
using Spotify.Web;
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

        [HttpGet]
        public IActionResult Login(string code)
        {
            if (code is null)
            {
                _logger.Debug("Need to redirect request to Spotify for authentication.");
                return Redirect(SpotifyUris.LoginUrl(_configuration));
            }
            else
            {
                var token = _tokens.CodeForAccessToken(code);
                var user = _spotify.Get(new GetUser(), token.AccessToken);

                var claims = new List<Claim>();
                claims.AddClaim(Names.Username, user.DisplayName);
                claims.AddClaim(Names.AccessToken, token.AccessToken);

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                HttpContext.SignInAsync(claimsPrincipal);

                _logger.Debug("Spotify user signed in is {Username}", user.DisplayName);
                return RedirectToAction("Index", "Spotify");
            }
        }
    }
}
