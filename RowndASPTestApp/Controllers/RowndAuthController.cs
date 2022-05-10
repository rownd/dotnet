using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rownd;
using Rownd.Helpers;
using Rownd.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RowndASPTestApp.Controllers
{
    [Route("/api/auth/rownd")]
    public class RowndAuthController : RowndCookieExchange
    {

        protected override async Task IsAllowedToSignIn(RowndUser rowndUser)
        {
            var userId = rowndUser.id;

            // throw new NotImplementedException("You have not purchased the product yet.");
        }

        public RowndAuthController(RowndClient client, ILogger<RowndAuthController> logger, UserManager<IdentityUser> userManager) : base(client, logger)
        {
            _userManager = userManager;
            _addNewUsersToDatabase = true;
        }
    }
}

