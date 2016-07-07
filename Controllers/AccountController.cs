#define DEBUG
using System;
using System.Collections.Specialized;
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
            var client = new RestClient("https://www.googleapis.com/oauth2/");

            var request = new RestRequest("v3/token", Method.POST) ;

request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("code", model.Code);
            request.AddParameter("client_id", "407408718192.apps.googleusercontent.com");
            request.AddParameter("client_secret", "prZAAMAj0qEu5v8YIXgkZqmR");

            var resp =   client.Execute<dynamic>(request);

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                request = new RestRequest("v2/userinfo", Method.POST) { RequestFormat = DataFormat.Json };
                request.AddHeader("Authorization", $"Bearer {resp.Data.access_token}");

                resp = client.Execute<dynamic>(request);

                if (resp.StatusCode != HttpStatusCode.OK)
                {
                    var userInfo =new Tuple<string,string>(resp.Data.id,resp.Data.locale);
                 var user=   _context.Accounts.FirstOrDefault(x => x.ID == userInfo.Item1);

                    if (user == null)
                    {
                        _context.Accounts.Add(new Account() {ID = userInfo.Item1, Location = userInfo.Item2});
                        _context.SaveChanges();

                        return Ok(new AuthProvider().GetKey(userInfo.Item1));
                    }
                    else
                    {
                        return Ok(new AuthProvider().GetKey(userInfo.Item1));
                    }
                }
                else
                {
                    return BadRequest("Authorization error");
                }
            }
            else
            {
                return BadRequest("Code not valid");
            }
        }

        public class AuthModel
        {
            [Required]
            public string Code;
        }
    }
}
