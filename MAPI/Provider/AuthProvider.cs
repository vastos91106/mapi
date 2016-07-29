using System;
using MAPI.Models;
using ServiceStack.Redis;

namespace MAPI.Provider
{
    public class AuthProvider
    {
        public string SetKey(Account account)
        {
            using (var redis = new RedisClient("188.227.17.24"))
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var stringChars = new char[8];
                var random = new Random();

                for (var i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                var finalString = new String(stringChars);

                redis.As<Account>().SetEntry(finalString, account);

                return finalString;
            }
        }

        public Account GetKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            using (var redis = new RedisClient("188.227.17.24"))
            {
                var session = redis.As<Account>();
                return session.GetValue(key);
            }
        }
    }
}