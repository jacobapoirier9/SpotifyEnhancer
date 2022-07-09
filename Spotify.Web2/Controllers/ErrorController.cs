using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spotify.Web2.Controllers
{
    public class ErrorController : Controller
    {

        public IActionResult InternalServerError() => View();

        public IActionResult NotFound => View();
    }
}
