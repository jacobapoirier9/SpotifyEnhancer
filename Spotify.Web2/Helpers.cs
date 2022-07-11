using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Spotify.Web
{
    public static class Helpers
    {
        public static void AddClaim<T>(this ICollection<Claim> claims, string claimType, T value)
        {
            if (value is IConvertible convertable)
                claims.Add(new Claim(claimType, convertable.ToString()));
            else
                claims.AddClaim(claimType, value.ToJson());
        }


        public static T Claim<T>(this Controller controller, string claimType) => controller.User.Claim<T>(claimType);
        public static T Claim<T>(this ClaimsPrincipal principal, string claimType)
        {
            var type = typeof(T);
            if (type.IsAssignableFrom(typeof(IConvertible)))
            {
                var value = principal.Claims.First(c => c.Type == claimType).Value;
                return (T)Convert.ChangeType(value, type);
            }
            else
            {
                var value = principal.Claims.First(c => c.Type == claimType).Value;
                return value.FromJson<T>();
            }
        }

        public static void ExecuteInChunks<T>(this IEnumerable<T> items, int chunkSize, Action<List<T>> operationPerChunk)
        {
            var list = items.ToList();
            for (var i = 0; i < list.Count; i += chunkSize)
            {
                // Just don't ask me how this works :)
                var count =
                    i + chunkSize > list.Count ?
                        (list.Count - chunkSize < 0 ?
                            list.Count :
                            list.Count - chunkSize) :
                        chunkSize;

                var range = list.GetRange(i, count);
                operationPerChunk.Invoke(range);
            }
        }


        public static List<T> AsList<T>(this T item)
        {
            var list = new List<T>() { item };
            return list;
        }
    }
}
