using ServiceStack;
using System.Collections.Generic;

namespace Spotify.Library.Core
{
    public interface IReturnPagable<T> : IReturn<T>
    {
        public short Limit { get; set; }

        public short Offset { get; set; }
    }

    public class PagableResponse<T>
    {
        public string Href { get; set; }

        public List<T> Items { get; set; }

        public short Limit { get; set; }

        public string Next { get; set; }

        public short Offset { get; set; }

        public string Previous { get; set; }

        public int Total { get; set; }
    }
}
