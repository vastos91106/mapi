#define DEBUG
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using MAPI.Models;
using MAPI.Provider;
using RestSharp;
using RestSharp.Extensions;

namespace MAPI.Controllers
{
    public class AccountController : ApiController
    {
        private Context _context = new Context();

        [Route("account/")]
        public IHttpActionResult Post(AuthModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("token required");

            var client = new RestClient("https://www.googleapis.com/oauth2/v1/");
            var request = new RestRequest("tokeninfo?", Method.GET);
            request.AddParameter("access_token", model.token);

            var response = client.Execute<dynamic>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string UserId = response.Data["user_id"];

                var sessionKey = string.Empty;

                if (_context.AccountTypes.Any(x => x.ID == UserId && x.AuthType == 1))
                {
                    sessionKey =
                        new AuthProvider().SetKey(
                            _context.AccountTypes.First(x => x.ID == UserId && x.AuthType == 1).Account);
                }
                else
                {
                    client = new RestClient("https://www.googleapis.com/userinfo/v2/me");
                    request = new RestRequest(Method.GET);
                    request.AddHeader("Authorization", $"Bearer {model.token}");

                    _context.Configuration.AutoDetectChangesEnabled = false;
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            var data = client.Execute<dynamic>(request).Data;

                            var account = new Account()
                            {
                                Location = data["locale"],
                                Name = data["name"]
                            };

                            _context.Accounts.Add(account);

                            _context.SaveChanges();

                            _context.AccountTypes.Add(new AccountType()
                            {
                                AccountID = account.ID,
                                AuthType = 1,
                                ID = UserId,
                            });

                            _context.SaveChanges();

                            transaction.Commit();

                            if (data["picture"] != null)
                            {
                                client = new RestClient(data["picture"]);
                                client.DownloadData(request).SaveAs(HttpContext.Current.Server.MapPath($"~/files/{account.ID}.jpg"));
                            }

                            sessionKey = new AuthProvider().SetKey(account);

                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Ok(ex.StackTrace);
                        }
                    }
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

            var user = new AuthProvider().GetKey(auth);
            if (user == null)
            {
                return Unauthorized();
            }
            else
            {
                return Ok(new
                {
                    id = user.ID,
                    authType = "Google",
                    name = user.Name,
                    avatar = Url.Link("GetUserAvatar", null)
                });
            }
        }

        [Route("account/avatar/", Name = "GetUserAvatar")]
        public IHttpActionResult GetAvatar()
        {
            var auth = Request.Headers.Authorization?.Scheme;

            var user = new AuthProvider().GetKey(auth);
            if (user == null)
            {
                return Unauthorized();
            }

            var path = HttpContext.Current.Server.MapPath($"~/files/{user.ID}.jpg");

            HttpResponseMessage result;
            if (!File.Exists(path))
            {
                result = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("File not found")
                };
                return new ResponseMessageResult(result);
            }

            HttpContent content = new StreamContent(new FileStream(path: path, mode: FileMode.Open, access: FileAccess.Read));
            content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = $"avatar{user.ID}.jpg"
            };

            result = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = content
            };

            return new ResponseMessageResult(result);
        }

        public class AuthModel
        {
            [Required]
            public string token;
        }
    }
}
