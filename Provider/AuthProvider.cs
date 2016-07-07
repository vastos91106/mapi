using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using MAPI.Models;

namespace MAPI.Provider
{
    public class AuthProvider
    {
        public string GetKey(string id)
        {
            var cache = MemoryCache.Default;

            if (cache.Any(x => (string) x.Value == id))
            {
                return cache.FirstOrDefault(x => (string) x.Value == id).Key;
            }

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddDays(7);

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);

            cache.Add(new CacheItem(finalString, id),policy);

            return finalString;
        }

        public bool IsAuthorize(string key)
        {
         return MemoryCache.Default.Any(x=>x.Key=="Session" && (string) x.Value==key);
        }
    }
}