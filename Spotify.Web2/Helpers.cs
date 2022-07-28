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
            var list = Enumerable.ToList(items);
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


        public static List<T> PutInList<T>(this T item)
        {
            var list = new List<T>() { item };
            return list;
        }

        /// <summary>
        /// Generate a random list of items from an existing list of items
        /// </summary>
        public static List<T> GetRandom<T>(this IEnumerable<T> items, int numberOfItems)
        {
            var toReturn = new List<T>(numberOfItems);
            var indexes = new List<int>(numberOfItems);

            var i = 0;
            var min = 0;
            var max = items.Count() - 1;
            var rand = new Random();

            while (i < numberOfItems)
            {
                var current = rand.Next(min, max);
                if (!indexes.Contains(current))
                {
                    indexes.Add(current);
                    toReturn.Add(items.ElementAt(current));
                    i++;
                }
            }

            return toReturn.ToList();
        }

        /// <summary>
        /// Gets a random element from a list
        /// </summary>
        public static T GetRandom<T>(this IEnumerable<T> items)
        {
            return items.GetRandom(1).First();
        }
    }
}
