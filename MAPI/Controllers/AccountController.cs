#define DEBUG
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Http;
using MAPI.Models;
using MAPI.Provider;
using RestSharp;

namespace MAPI.Controllers
{
    public class AccountController : ApiController
    {
        private Context _context = new Context();

        [Route("account/")]
        public IHttpActionResult Post(AuthModel model)
        {
            var client = new RestClient("https://www.googleapis.com/oauth2/v1/");
            var request = new RestRequest("tokeninfo?", Method.GET);
            request.AddParameter("access_token", model.Token);

            var response = client.Execute<dynamic>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string UserId = response.Data["user_id"];
                
                var sessionKey = string.Empty;
             
                if (_context.AccountTypes.Any(x => x.ID == UserId && x.AuthType == 1))
                {
                    sessionKey =
                        new AuthProvider().SetKey(
                            _context.AccountTypes.First(x => x.ID == UserId && x.AuthType == 1).AccountID.ToString());
                }
                else
                {
                    var account = new Account();

                    _context.Accounts.Add(account);
                    _context.SaveChanges();

                    _context.AccountTypes.Add(new AccountType()
                    {
                        AccountID = account.ID,
                        AuthType = 1,
                        ID = UserId
                    });
                    _context.SaveChanges();
                    sessionKey = new AuthProvider().SetKey(account.ID.ToString());
                }

                return Ok(sessionKey);
                }
            else
            {
                return BadRequest("Token Invalid");
            }
        }

        [Route("account/")]
        public IHttpActionResult Get()
        {
            var auth = Request.Headers.Authorization?.Scheme;

            var key = new AuthProvider().GetKey(auth);
            if (key==null)
            {
                return Unauthorized();
            }
            else
            {
                return Ok(key);
            }
        }

        public class AuthModel
        {
            [Required]
            public string Token;
        }
    }
}
