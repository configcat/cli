using System.Linq;

namespace System.Collections.Generic
{
    public static class ListExtensions
    {
        public static List<List<T>> Split<T>(this List<T> source, int chunkSize)
        {
            if (source is null || source.Count == 0)
                return new List<List<T>>();

            if (chunkSize > source.Count || chunkSize == 0)
                return new List<List<T>> { source };

            var whole = source.Count / chunkSize;
            var remaining = source.Count % chunkSize;

            var from = 0;
            var splittedSize = remaining == 0 ? whole : whole + 1;
            var splitted = new List<List<T>>(splittedSize);
            for (int i = splittedSize; i-- > 0;)
            {
                if (remaining != 0 && i == 0)
                    chunkSize = remaining;

                splitted.Add(new List<T>(source.GetRange(from, chunkSize)));
                from += chunkSize;
            }

            return splitted;
        }

        public static List<List<T>> SplitFilled<T>(this List<T> source, int chunkSize)
        {
            if (source is null || source.Count == 0)
                return new List<List<T>>();

            if (chunkSize > source.Count || chunkSize == 0)
                return new List<List<T>> { source };

            var splitted = source.Split(chunkSize);
            var last = splitted[splitted.Count - 1];
            if(last.Count < chunkSize)
                last.AddRange(Enumerable.Repeat<T>(default, chunkSize - last.Count));

            return splitted;
        }

        public static int PageIndexOf<T>(this List<List<T>> source, T item)
        {
            if (source is null || source.Count == 0)
                return -1;

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].IndexOf(item) != -1)
                    return i;
            }

            return -1;
        }
    }
}
