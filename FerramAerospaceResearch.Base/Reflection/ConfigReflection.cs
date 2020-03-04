using System;
using System.Collections.Generic;
using System.Linq;

namespace FerramAerospaceResearch.Reflection
{
    public class ReflectedConfig
    {
        public object Instance { get; set; }
        public NodeReflection Reflection { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfigReflection : Singleton<ConfigReflection>
    {
        /// <summary>
        ///     Dictionary of name - config pairs
        /// </summary>
        private readonly Dictionary<string, ReflectedConfig> nodes = new Dictionary<string, ReflectedConfig>();

        private bool initialized;

        public Dictionary<string, ReflectedConfig> Configs
        {
            get
            {
                if (!initialized)
                    Initialize();
                return nodes;
            }
        }

        public void Initialize(bool force = false)
        {
            if (initialized && !force)
                return;

            try
            {
                var allNodes =
                    new List<Pair<ConfigNodeAttribute, Type>>(ReflectionUtils
                                                                  .FindAttribute<ConfigNodeAttribute>(false));

                foreach (Pair<ConfigNodeAttribute, Type> pair in allNodes)
                {
                    if (!pair.First.IsRoot)
                        continue;
                    var node = NodeReflection.GetReflection(pair.Second);
                    nodes.Add(node.Id,
                              new ReflectedConfig
                              {
                                  Instance = ReflectionUtils.FindInstance(pair.Second),
                                  Reflection = node
                              });
                }

                FARLogger.TraceFormat("Config nodes found: {0}", string.Join(", ", nodes.Select(p => p.Key)));
            }
            finally
            {
                initialized = true;
            }
        }
    }
}
