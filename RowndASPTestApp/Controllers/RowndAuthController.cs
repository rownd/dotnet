using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rownd;
using Rownd.Helpers;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RowndASPTestApp.Controllers
{
    [Route("/api/auth/rownd")]
    public class RowndAuthController : RowndCookieExchange
    {

        public RowndAuthController(RowndClient client, ILogger<RowndAuthController> logger, UserManager<IdentityUser> userManager) : base(client, logger)
        {
            _userManager = userManager;
            _addNewUsersToDatabase = true;
        }
    }
}

