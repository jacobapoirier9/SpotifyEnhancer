using NLog;
using ServiceStack;
using Spotify.Library.Core;
using System;
using System.Collections.Generic;

namespace Spotify.Web
{
    public static class SpotifyServiceClientExtensions
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public static List<TOut> GetAll<TResponse, TOut>(this IServiceClient client, IReturnPagable<TResponse> request, Func<TResponse, PagableResponse<TOut>> func)
        {
            var list = new List<TOut>();
            var response = default(TResponse);
            var pager = default(PagableResponse<TOut>);

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
