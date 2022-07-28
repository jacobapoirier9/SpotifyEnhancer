using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using Spotify.Library.Core;

namespace Spotify.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IServiceClient _spotify;
        public HomeController(IServiceClient spotify)
        {
            _spotify = spotify;
        }


        private void SetupApi() => SetupApi(out _);
        private void SetupApi(out string username)
        {
            username = this.Claim<string>(Names.Username);
            var token = this.Claim<string>(Names.AccessToken);

            _spotify.BearerToken = token;
        }

        public IActionResult Index()
        {
            SetupApi();

            var playlists = _spotify.GetAll(new GetPlaylists(), response => response);
            return View(playlists);
        }
    }
}
