using System.Collections.Generic;

namespace FerramAerospaceResearch
{
    public abstract class FARConfigProvider
    {
        public Dictionary<string, FARConfigParser> Parsers { get; } = new Dictionary<string, FARConfigParser>();
        public abstract IConfigNode[] LoadConfigs(string name);
        public abstract IConfigNode CreateNode();
        public abstract IConfigNode CreateNode(string name);
        public abstract IConfigNode CreateNode(string name, string vcomment);
    }
}
