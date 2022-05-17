using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rownd;
using Rownd.Helpers;
using Rownd.Models;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RowndASPTestApp.Controllers
{
    [Route("/api/auth/rownd")]
    public class RowndAuthController : RowndCookieExchange
    {
        // Only used for deserialization in this example, since the WooCommerceNET lib doesn't support basic auth
        static RestAPI rest = new RestAPI("http://www.yourstore.com/wp-json/wc/v3/", "<WooCommerce OAuth Key>", "<WooCommerce OAuth Secret>");

        protected async Task<string> GetStreamContent(Stream s, string charset)
        {
            StringBuilder sb = new StringBuilder();
            byte[] Buffer = new byte[512];
            int count = 0;
            count = await s.ReadAsync(Buffer, 0, Buffer.Length).ConfigureAwait(false);
            while (count > 0)
            {
                sb.Append(Encoding.GetEncoding(charset).GetString(Buffer, 0, count));
                count = await s.ReadAsync(Buffer, 0, Buffer.Length).ConfigureAwait(false);
            }

            return sb.ToString();
        }

        protected override async Task IsAllowedToSignIn(RowndUser rowndUser)
        {
            //string? userId = rowndUser?.data?["user_id"].ToString();

            //var handler = new HttpClientHandler();
            //handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            //using (var httpClient = new HttpClient(handler)
            //{
            //    DefaultRequestHeaders =
            //    {
            //        Authorization = new BasicAuthenticationHeaderValue("ck_d4c166c8682859917a4a7762e3a8187fa1c5fd81", "cs_451ea5b9e96e62a4d26c30bbe6531e583dbcb737")
            //    }
            //})
            //{
            //    var wcBaseUrl = "https://rowndauthtesting.local/wp-json/wc/v3";
            //    var requiredProductId = "14";
                
            //    var request = new HttpRequestMessage()
            //    {
            //        RequestUri = new Uri($"{wcBaseUrl}/customers?rownd_id={userId}"),
            //        Method = HttpMethod.Get,
            //    };

            //    var resp = await httpClient.SendAsync(request);

            //    var jsonStr = await GetStreamContent(resp.Content.ReadAsStream(), "utf-8");
            //    var customers = rest.DeserializeJSon<List<Customer>>(jsonStr);
            //    if (customers.Count != 1)
            //    {
            //        throw new Exception("A customer profile could not be found for this user.");
            //    }

            //    request = new HttpRequestMessage()
            //    {
            //        RequestUri = new Uri($"{wcBaseUrl}/orders?product={requiredProductId}&customer={customers[0].id}"),
            //        Method = HttpMethod.Get,
            //    };
            //    resp = await httpClient.SendAsync(request);

            //    jsonStr = await GetStreamContent(resp.Content.ReadAsStream(), "utf-8");
            //    var orders = rest.DeserializeJSon<List<Order>>(jsonStr);
            //    if (orders?.Count != 1)
            //    {
            //        throw new Exception("No orders were found containing the required products or entitlements.");
            //    }
            //}

            // throw new NotImplementedException("You have not purchased the product yet.");
        }

        public RowndAuthController(RowndClient client, ILogger<RowndAuthController> logger, UserManager<IdentityUser> userManager) : base(client, logger)
        {
            _userManager = userManager;
            _addNewUsersToDatabase = true;
        }
    }
}

