using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceStack.Data;
using Spotify.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Spotify.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDbConnectionFactory _factory;

        public AuthController(ILogger<HomeController> logger, IDbConnectionFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            return View();
        }
    }
}
