using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using MAPI.Models;
using MAPI.Provider;
using ServiceStack.DataAnnotations;
using ServiceStack.Redis;

namespace MAPI.Controllers
{
    public class MarkController : ApiController
    {
        private Context _context = new Context();

        [Route("marks/")]
        public IHttpActionResult PostMarks(GeoPoint model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var viewModel = new List<object>();

            using (var redis = new RedisClient("212.116.121.56"))
            {
                var result = redis.GeoRadius(key: "marks", longitude: model.lon, latitude: model.lat, radius: model.radius, unit: RedisGeoUnit.Kilometers);

                foreach (var redisGeoResult in result)
                {
                    var detail =
                        redis.As<Mark>()
                            .GetValue(redisGeoResult.Member);

                    var mark = new
                    {
                        lat = detail.Lat,
                        lon = detail.Lon,
                        name = detail.Name,
                        avatar = Url.Link("GetMarkAvatar", new { id = detail.ID }),
                        rating = detail.Rating,
                        description = detail.Description
                    };

                    viewModel.Add(mark);
                }
            }

            if (!viewModel.Any())
                return NotFound();

            return Ok(viewModel);
        }

        [Route("mark/{id}")]
        public IHttpActionResult GetMark(int id)
        {
            var auth = Request.Headers.Authorization?.Scheme;

            var user = new AuthProvider().GetKey(auth);

            if (user == null)
            {
                return Unauthorized();
            }

            Mark mark;

            using (var redis = new RedisClient("212.116.121.56"))
            {
                mark =
                     redis.As<Mark>()
                         .GetValue(id.ToString());
            }

            if (mark == null)
            {
                return NotFound();
            }

            var viewMark = new
            {
                lat = mark.Lat,
                lon = mark.Lon,
                name = mark.Name,
                avatar = Url.Link("GetMarkAvatar", new { id = mark.ID }),
                rating = mark.Rating,
                description = mark.Description
            };

            return Ok(viewMark);
        }

        [Route("mark/")]
        public IHttpActionResult PostMark(MarkViewModel model)
        {
            var auth = Request.Headers.Authorization?.Scheme;

            var user = new AuthProvider().GetKey(auth);

            if (user == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var mark = new Mark
            {
                Description = model.description,
                Lat = model.lat,
                Lon = model.lon,
                Name = model.name,
                Radius = 1,
                Rating = 0,
                AccountID = user.ID
            };

            _context.Marks.Add(mark);

            _context.SaveChanges();

            if (model.avatar != null)
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(model.avatar);
                    MemoryStream ms = new MemoryStream(imageBytes, 0,
                      imageBytes.Length);

                    // Convert byte[] to Image
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    Image image = Image.FromStream(ms, true);

                    image.Save(HttpContext.Current.Server.MapPath($"~/files/mark/{mark.ID}.jpg"));
                }
                catch (Exception e) { }
            }

            _context.Entry(mark).State = EntityState.Modified;
            _context.SaveChanges();

            using (var redis = new RedisClient("212.116.121.56"))
            {
                redis.GeoAdd("marks", mark.Lon, mark.Lat, mark.ID.ToString());
            }

            using (var redis = new RedisClient("212.116.121.56"))
            {
                redis.As<Mark>().SetEntry(mark.ID.ToString(), mark);
            }

            return Ok(new { id = mark.ID });
        }

        [Route("mark/{id}/")]
        public IHttpActionResult PutMark(int id, [FromBody] MarkViewModel model)
        {
            var auth = Request.Headers.Authorization?.Scheme;

            var user = new AuthProvider().GetKey(auth);

            if (user == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbMark = _context.Marks.FirstOrDefault(x => x.ID == id);

            if (dbMark == null)
                return NotFound();

            dbMark.Name = model.name;
            dbMark.Description = model.description;

            if (model.avatar != null)
            {
                var imageBytes = Convert.FromBase64String(model.avatar);
                var ms = new MemoryStream(imageBytes, 0,
                  imageBytes.Length);

                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                var image = Image.FromStream(ms, true);

                image.Save(HttpContext.Current.Server.MapPath($"~/files/mark/{dbMark.ID}.jpg"));
            }

            _context.Entry(dbMark).State = EntityState.Modified;
            _context.SaveChanges();

            using (var redis = new RedisClient("212.116.121.56"))
            {
                redis.As<Mark>().SetEntry(dbMark.ID.ToString(), dbMark);
            }

            return Ok(new { id = dbMark.ID });
        }


        [Route("mark/avatar/{id}", Name = "GetMarkAvatar")]
        public IHttpActionResult GetAvatar(int id)
        {
            var auth = Request.Headers.Authorization?.Scheme;

            var user = new AuthProvider().GetKey(auth);
            if (user == null)
            {
                return Unauthorized();
            }

            var path = HttpContext.Current.Server.MapPath($"~/files/mark/{id}.jpg");

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
                FileName = $"mark{id}.jpg"
            };

            result = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = content
            };

            return new ResponseMessageResult(result);
        }


        public class MarkViewModel
        {
            [System.ComponentModel.DataAnnotations.Required]
            public string name;
            public string avatar;
            [System.ComponentModel.DataAnnotations.Required]
            [Range(double.MinValue, double.MaxValue)]
            public double lat;
            [System.ComponentModel.DataAnnotations.Required]
            [Range(double.MinValue, double.MaxValue)]
            public double lon;
            [System.ComponentModel.DataAnnotations.Required]
            public string description;
        }

        public class GeoPoint
        {
            [System.ComponentModel.DataAnnotations.Required]
            public double lat;
            [System.ComponentModel.DataAnnotations.Required]
            public double lon;
            [System.ComponentModel.DataAnnotations.Required]
            public double radius;
        }
    }
}
