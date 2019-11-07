using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FerramAerospaceResearch
{
    public interface IConfigValue
    {
        string Name { get; }

        string EditableName { get; }
        event Action<IConfigValue> onChanged;
        void Reset();
        void DebugString(StringBuilder sb);

        void Parse(IConfigNode node);
        void Save(IConfigNode node);
    }

    public interface IConfigValue<T> : IConfigValue
    {
        T Default { get; }
        T Value { get; set; }
        event Action<IConfigValue<T>> onValueChanged;
    }

    // abstract class since non-generic methods don't accept generic arguments
    public abstract class ConfigValue<T> : IConfigValue<T>
    {
        private readonly Func<T, T> transform;

        // using field for reference semantics
        protected T value;

        protected ConfigValue(string name, T value)
        {
            transform = null;
            Name = name;
            Default = value;
            this.value = Default;
        }

        protected ConfigValue(string name, T value, Func<T, T> transform) : this(name, transform(value))
        {
            this.transform = transform;
        }

        public event Action<IConfigValue> onChanged;
        public event Action<IConfigValue<T>> onValueChanged;
        public string Name { get; }
        public T Default { get; }

        public T Value
        {
            get { return value; }
            set
            {
                T old = this.value;
                this.value = transform == null ? value : transform(value);
                if (EqualityComparer<T>.Default.Equals(this.value, old))
                    return;

                onChanged?.Invoke(this);
                onValueChanged?.Invoke(this);
            }
        }

        public void Parse(IConfigNode node)
        {
            T old = value;
            ParseValue(node);
            value = old;
            Value = value;
        }

        public abstract void Save(IConfigNode node);

        public string EditableName
        {
            get { return $"%{Name}"; }
        }

        public void Reset()
        {
            Value = Default;
        }

        public void DebugString(StringBuilder sb)
        {
            sb.Append("    ").Append(Name).Append(": ").AppendLine(Value.ToString());
        }

        protected abstract void ParseValue(IConfigNode node);

        // implicit conversion to the stored value
        public static implicit operator T(ConfigValue<T> cv)
        {
            return cv.Value;
        }
    }

    // boilerplate for concrete types
    public class IntConfigValue : ConfigValue<int>
    {
        public IntConfigValue(string name, int value) : base(name, value)
        {
        }

        public IntConfigValue(string name, int value, Func<int, int> transform) : base(name, value, transform)
        {
        }

        protected override void ParseValue(IConfigNode node)
        {
            node.TryGetValue(Name, ref value);
        }

        public override void Save(IConfigNode node)
        {
            node.AddValue(Name, Value);
        }
    }

    public class StringConfigValue : ConfigValue<string>
    {
        public StringConfigValue(string name, string value) : base(name, value)
        {
        }

        public StringConfigValue(string name, string value, Func<string, string> transform) :
            base(name, value, transform)
        {
        }

        protected override void ParseValue(IConfigNode node)
        {
            node.TryGetValue(Name, ref value);
        }

        public override void Save(IConfigNode node)
        {
            node.AddValue(Name, Value);
        }
    }

    public class FloatConfigValue : ConfigValue<float>
    {
        public FloatConfigValue(string name, float value) : base(name, value)
        {
        }

        public FloatConfigValue(string name, float value, Func<float, float> transform) : base(name, value, transform)
        {
        }

        protected override void ParseValue(IConfigNode node)
        {
            node.TryGetValue(Name, ref value);
        }

        public override void Save(IConfigNode node)
        {
            node.AddValue(Name, Value);
        }
    }

    public class ColorConfigValue : ConfigValue<Color>
    {
        private static readonly char[] separators = {',', ' ', ';'};

        public ColorConfigValue(string name, Color value) : base(name, value)
        {
        }

        protected override void ParseValue(IConfigNode node)
        {
            string color = string.Empty;
            node.TryGetValue(Name, ref color);
            Value = ReadColor(color);
        }

        public override void Save(IConfigNode node)
        {
            node.AddValue(Name, SaveColor(Value));
        }

        private static Color ReadColor(string input)
        {
            string[] splitValues = input.Split(separators);

            int curIndex = 0;
            var color = new Color {a = 1};
            foreach (string s in splitValues)
            {
                if (s.Length <= 0)
                    continue;
                if (!float.TryParse(s, out float val))
                    continue;
                switch (curIndex)
                {
                    case 0:
                        color.r = val;
                        break;
                    case 1:
                        color.g = val;
                        break;
                    default:
                        color.b = val;
                        return color;
                }

                curIndex++;
            }

            return color;
        }

        private static string SaveColor(Color color)
        {
            var builder = new StringBuilder();

            //Should return string in format of color.r, color.g, color.b
            builder.Append(color.r);
            builder.Append(",");
            builder.Append(color.g);
            builder.Append(",");
            builder.Append(color.b);

            return builder.ToString();
        }
    }
}
