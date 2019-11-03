namespace FerramAerospaceResearch
{
    public struct ConfigValue<T>
    {
        public string Name { get; }
        public T Value { get; }

        public string EditableName
        {
            get { return $"%{Name}"; }
        }

        public ConfigValue(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }
}
