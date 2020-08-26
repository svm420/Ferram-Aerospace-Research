using System;

namespace FerramAerospaceResearch
{
    public static class FARUtils
    {
        public static bool ValuesEqual<T>(T a, T b)
        {
            return a switch
            {
                IComparable<T> comparable => (comparable.CompareTo(b) == 0),
                IEquatable<T> equatable => equatable.Equals(b),
                null => b is null,
                IComparable comparable => (comparable.CompareTo(b) == 0),
                _ => a.Equals(b)
            };
        }

        public static bool ValuesEqual<T>(object a, T b)
        {
            return a switch
            {
                IComparable<T> comparable => (comparable.CompareTo(b) == 0),
                IEquatable<T> equatable => equatable.Equals(b),
                null => b is null,
                IComparable comparable => (comparable.CompareTo(b) == 0),
                _ => a.Equals(b)
            };
        }

        public static bool ValuesEqual(object a, object b)
        {
            return a switch
            {
                IComparable comparable => (comparable.CompareTo(b) == 0),
                null => b is null,
                _ => a.Equals(b)
            };
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T c = b;
            b = a;
            a = c;
        }
    }
}
