using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RowndASPTestApp.Controllers
{
    [Authorize(Roles = "admin")]
    public class RoleProtectedController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            var roles = ((ClaimsIdentity)User.Identity).Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

            var roleString = String.Join(",", roles.ToList<string>());
            ViewBag.Roles = roleString;

            return View();
        }
    }
}

