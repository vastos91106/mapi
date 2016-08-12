using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using MAPI.Models;
using MAPI.Provider;
using Newtonsoft.Json;
using NGeoHash.Portable;
using ServiceStack.DataAnnotations;
using ServiceStack.Redis;

namespace MAPI.Controllers
{
    public class MarkController : ApiController
    {
        private Context _context = new Context();

        [Route("mark/")]
        public IHttpActionResult GetMarks([FromUri]GeoPoint model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var viewModel = new List<object>();

            using (var redis = new RedisClient("212.116.121.56"))
            {
                var result = redis.FindGeoResultsInRadius(key: "marks", longitude: model.lon, latitude: model.lat, radius: model.radius, unit: RedisGeoUnit.Kilometers, count: null, sortByNearest: true);
                foreach (var redisGeoResult in result)
                {
                    var detail = JsonConvert.DeserializeObject<dynamic>(redisGeoResult.Member);
                    var mark = new
                    {
                        lat = redisGeoResult.Latitude,
                        lon = redisGeoResult.Longitude,
                        name = detail["name"],
                        avatar = detail["avatar"],
                        rating = detail["rating"],
                        description = detail["description"]
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

            var mark = _context.Marks.FirstOrDefault(x => x.ID == id);

            if (mark == null)
            {
                return NotFound();
            }

            var viewMark = new
            {
                lat = mark.Lat,
                lon = mark.Lon,
                name = mark.Name,
                avatar = mark.Avatar,
                rating = mark.Rating,
                description = mark.Description
            };

            return Ok(viewMark);
        }

        [Route("mark/")]
        public IHttpActionResult PostMark([FromBody] Mark model)
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
                Avatar = model.Avatar,
                Description = model.Avatar,
                Lat = model.Lat,
                Lon = model.Lon,
                Name = model.Name,
                Radius = 1,
                Rating = 0,
                AccountID = user.ID
            };

            _context.Marks.Add(mark);

            _context.SaveChanges();

            using (var redis = new RedisClient("212.116.121.56"))
            {
                var obj = new
                {
                    name = mark.Name,
                    avatar = mark.Avatar,
                    rating = mark.Rating,
                    description = mark.Description
                };

                redis.AddGeoMember("marks", model.Lon, model.Lat, JsonConvert.SerializeObject(obj));
            }

            return Ok(new { id = mark.ID });
        }

        [Route("mark/{id}/")]
        public IHttpActionResult PutMark(int id, [FromBody] Mark model)
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

            dbMark.Avatar = model.Avatar;
            dbMark.Name = model.Avatar;
            dbMark.Description = model.Avatar;

            _context.Entry(dbMark).State = EntityState.Modified;
            _context.SaveChanges();

            using (var redis = new RedisClient("212.116.121.56"))
            {
                var geoHash = GeoHash.Encode(dbMark.Lat, dbMark.Lon);
                var redisMark = redis.As<RedisGeoResult>().GetValue(geoHash);

                var serMark = JsonConvert.DeserializeObject<MarkInfo>(redisMark.Member);

                serMark.name = dbMark.Name;
                serMark.avatar = dbMark.Avatar;
                serMark.description = dbMark.Avatar;
                serMark.rating = dbMark.Rating;

                redisMark.Member = JsonConvert.SerializeObject(serMark);

                redis.Store(redisMark);
            }

            return Ok(new { id = dbMark.ID });
        }

        public class MarkInfo
        {
            public string name;
            public string avatar;
            public int rating;
            public string description;
        }

        public class GeoPoint
        {
            [Required]
            public double lat;
            [Required]
            public double lon;
            [Required]
            public double radius;
        }
    }
}
