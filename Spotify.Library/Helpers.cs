using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spotify.Library
{
    public static class Helpers
    {
        public static string BuildUri(this string uri, object queryParams)
        {
            if (!uri.Contains('?'))
                uri += '?';

            foreach (var queryParam in queryParams.GetType().GetProperties())
            {
                if (uri.Contains(queryParam.Name + '='))
                {
                    uri = Regex.Replace(uri, queryParam.Name + "=\\w*", queryParam.Name + '=' + queryParam.GetValue(queryParams));
                }
                else
                {
                    uri += '&' + queryParam.Name + '=' + queryParam.GetValue(queryParams);
                }
            }

            return uri;
        }

        public static string JoinToString(this IEnumerable<IConvertible> items, IConvertible seperator)
        {
            var toReturn = string.Empty;
            foreach (var item in items.Cast<string>())
            {
                toReturn += item + seperator;
            }

            return toReturn == string.Empty ? string.Empty : toReturn.Substring(0, toReturn.Length - seperator.ToString().Length);
        }

        public static T Get<T>(this IConfiguration configuration, string key, bool required = false)
        {
            try
            {
                var value = configuration[key];
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                if (required)
                    throw;
                else
                    return default(T);
            }
        }
    }
}
