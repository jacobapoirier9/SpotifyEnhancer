using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NLog;
using ServiceStack;
using Spotify.Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spotify.Web
{
    internal static class Helpers
    {
        public static object To(this IConvertible item, Type type)
        {
            return Convert.ChangeType(item, type);
        }

        /// <summary>
        /// Converts an object to a given type
        /// </summary>
        public static T To<T>(this object item) where T : IConvertible
        {
            return (T)Convert.ChangeType(item, typeof(T));
        }

        /// <summary>
        /// Converts a list of object to a given type
        /// </summary>
        public static List<T> To<T>(this IEnumerable<object> items) where T : IConvertible
        {
            var toReturn = new List<T>();
            foreach (var item in items)
            {
                toReturn.Add(((IConvertible)item).To<T>());
            }

            return toReturn;
        }

        public static void AddClaim<T>(this ICollection<Claim> claims, string claimType, T value)
        {
            if (value is IConvertible convertable)
                claims.Add(new Claim(claimType, convertable.ToString()));
            else
                claims.AddClaim(claimType, value.ToJson());
        }


        public static T Claim<T>(this IHttpContextAccessor context, string claimType) => context.HttpContext.User.Claim<T>(claimType);
        public static T Claim<T>(this Controller controller, string claimType) => controller.User.Claim<T>(claimType);
        public static T Claim<T>(this Controller controller) => controller.User.Claim<T>(typeof(T).Name);
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

        public static ArrayGrid<T> ToArrayGrid<T>(this IEnumerable<T> items, int numberOfCols)
        {
            var count = items.Count();
            var numberOfRows = count / numberOfCols;

            if (count > numberOfRows * numberOfCols)
                numberOfRows++;

            var e = items.GetEnumerator();
            var grid = new T?[numberOfRows, numberOfCols];
            for (var r = 0; r < numberOfRows; r++)
            {
                for (var c = 0; c < numberOfCols; c++)
                {
                    grid[r, c] = e.MoveNext() ? e.Current : default(T);
                }
            }

            return new ArrayGrid<T>()
            {
                Grid = grid,
                List = items.ToList(),
                NumberOfCols = numberOfCols,
                NumberOfRows = numberOfRows,
                Length = count
            };
        }

        public class ArrayGrid<T>
        {
            public T?[,] Grid { get; set; }

            public int NumberOfRows { get; set; }

            public int NumberOfCols { get; set; }

            public List<T> List { get; set; }

            public int Length { get; init; }
        }
    }

    public static class SpotifyServiceClientExtensions
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public static List<TOut> GetAll<TResponse, TOut>(this IServiceClient client, IReturnPagable<TResponse> request, Func<TResponse, SpotifyPagableResponse<TOut>> func)
        {
            var list = new List<TOut>();
            var response = default(TResponse);
            var pager = default(SpotifyPagableResponse<TOut>);

            do
            {
                response = client.Get(request);
                pager = func(response);
                list.AddRange(pager.Items);
                request.Offset += pager.Limit;

                //_logger.Trace("Offset: {Offset}, Limit: {Limit}, Total: {Total}", pager.Offset, pager.Limit, pager.Total);
            } while (pager.Next is not null && list.Count < pager.Total);
            return list;
        }

        public static TResponse Get<TResponse>(this IServiceClient client, IReturn<TResponse> request, string token)
        {
            client.BearerToken = token;
            return client.Get(request);
        }
    }
}
