using Microsoft.AspNetCore.Authentication;
using Rownd.Core;
using Rownd.Models;

namespace Rownd.Helpers
{
    public class RowndAuthOptions : AuthenticationSchemeOptions
    {
        public bool ErrOnInvalidToken { get; set; } = true;
    }

}