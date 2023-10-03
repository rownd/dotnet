using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rownd;
using RowndASPTestApp.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RowndASPTestApp.Controllers
{
    [Authorize]
    public class ProtectedController : Controller
    {

        private readonly RowndClient _rowndClient;

        public ProtectedController(RowndClient rowndClient)
        {
            _rowndClient = rowndClient;
        }
        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var roles = ((ClaimsIdentity)User.Identity).Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

            var roleString = String.Join(",", roles.ToList<string>());
            ViewBag.Roles = roleString;

            var userId = ((ClaimsIdentity)User.Identity).Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .First();

            var rowndUserProfile = await _rowndClient.Users.GetProfile(userId);
            RowndUserProfile userProfile = new() {
                Id = rowndUserProfile.Id,
                Name = rowndUserProfile.Data?["first_name"]?.ToString()
            };

            return View(userProfile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([Bind("Name")] RowndUserProfile rowndUser)
        {
            if (ModelState.IsValid)
            {
                var userId = ((ClaimsIdentity)User.Identity).Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value)
                    .First();

                var rowndUserProfile = await _rowndClient.Users.GetProfile(userId);
                if (rowndUserProfile.Data == null) {
                    throw new Exception("User profile not found");
                }
                rowndUserProfile.Data["first_name"] = rowndUser?.Name;

                await _rowndClient.Users.UpdateProfile(rowndUserProfile);
                return RedirectToAction("Index");
            }

            return View("Index");
        }
    }
}

