using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rownd;
using RowndSharedLib.Helpers;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RowndASPTestApp.Controllers
{
    [Route("/api/auth/rownd")]
    public class RowndAuthController : RowndCookieExchange
    {
        public RowndAuthController(RowndClient client) : base(client)
        {
        }
    }
}

