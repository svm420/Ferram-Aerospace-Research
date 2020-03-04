namespace FerramAerospaceResearch
{
    public readonly struct Pair<Ta, Tb>
    {
        public readonly Ta First;
        public readonly Tb Second;

        public Pair(Ta first, Tb second)
        {
            First = first;
            Second = second;
        }
    }

    public static class Pair
    {
        // use type deduction to save on writing parameters every time
        public static Pair<Tx, Ty> Create<Tx, Ty>(Tx first, Ty second)
        {
            return new Pair<Tx, Ty>(first, second);
        }
    }
}
