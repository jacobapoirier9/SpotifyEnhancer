using System.Collections.Generic;

namespace Spotify.Web
{
    public class ArrayGrid<T>
    {
        public T[][] GridForDebugging
        {
            get
            {
                var e = List.GetEnumerator();
                var toReturn = new T[NumberOfRows][];
                for (var r = 0; r < NumberOfRows; r++)
                {
                    toReturn[r] = new T[NumberOfCols];
                    for (var c = 0; c < NumberOfCols; c++)
                    {
                        toReturn[r][c] = e.MoveNext() ? e.Current : default(T);
                    }
                }

                return toReturn;
            }
        }
        public T[,] Grid { get; set; }

        public int NumberOfRows { get; set; }

        public int NumberOfCols { get; set; }

        public List<T> List { get; set; }

        public int Length { get; init; }
    }
}
