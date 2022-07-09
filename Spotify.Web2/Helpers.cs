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
            var grid = new T[numberOfRows, numberOfCols];
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




        /// <summary>
        /// Repeats a string a number of times
        /// </summary>
        public static string Repeat(this string str, int count)
        {
            var toReturn = "";
            for (var i = 0; i < count; i++)
            {
                toReturn += str;
            }
            return toReturn;
        }

        /// <summary>
        /// Builds a minified json string
        /// </summary>
        public static string MinifyJson(string json)
        {
            var builder = new StringBuilder();
            var isInQuotes = false;
            var lastChar = char.MinValue;

            foreach (var letter in json)
            {
                if (isInQuotes)
                {
                    switch (letter)
                    {
                        case '"':
                            builder.Append(letter);
                            isInQuotes = false;
                            break;
                        default:
                            builder.Append(letter);
                            break;
                    }
                }
                else
                {
                    switch (letter)
                    {
                        case ' ':
                        case '\n':
                        case '\r':
                        case '\t':
                            break;

                        case '"':

                            if (lastChar == '\\')
                            {

                            }
                            else
                            {
                                builder.Append(letter);
                                isInQuotes = true;
                            }

                            break;
                        default:
                            builder.Append(letter);
                            break;
                    }
                }

                lastChar = letter;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Builds a nicely formatted json string
        /// </summary>
        public static string PrettyPrintJson(string json)
        {
            json = MinifyJson(json);

            var builder = new StringBuilder();
            var tabIndex = 0;
            var toRepeat = "\t";

            foreach (var letter in json)
            {
                switch (letter)
                {
                    case '{':
                    case '[':
                        tabIndex++;
                        builder.Append(letter);
                        builder.Append(Environment.NewLine + toRepeat.Repeat(tabIndex));
                        break;

                    case '}':
                    case ']':
                        tabIndex--;
                        builder.Append(Environment.NewLine + toRepeat.Repeat(tabIndex));
                        builder.Append(letter);
                        break;

                    case ':':
                        builder.Append(letter + " ");
                        break;

                    case ',':
                        builder.Append(letter);
                        builder.Append(Environment.NewLine + toRepeat.Repeat(tabIndex));
                        break;

                    default:
                        builder.Append(letter);
                        break;
                }
            }

            return builder.ToString();
        }

    }
}
