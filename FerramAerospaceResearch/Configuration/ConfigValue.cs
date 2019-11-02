namespace FerramAerospaceResearch
{
    public struct ConfigValue<T>
    {
        public string Name { get; }
        public T Value { get; }

        public ConfigValue(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }
}
