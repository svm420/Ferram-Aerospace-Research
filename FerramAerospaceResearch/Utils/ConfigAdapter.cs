using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FerramAerospaceResearch.Interfaces;
using FerramAerospaceResearch.Reflection;
using UnityEngine;

namespace FerramAerospaceResearch
{
    [FARAddon(700, true)]
    public class ConfigAdapter : MonoSingleton<ConfigAdapter>, IWaitForAddon, IReloadable
    {
        public int Priority { get; set; } = 999;

        public void DoReload()
        {
            StartCoroutine(DoLoadConfig());
        }

        public bool Completed { get; set; }

        private void Start()
        {
            StartCoroutine(DoSetup());
        }

        private IEnumerator DoSetup()
        {
            try
            {
                Task task = Task.Factory.StartNew(() => ConfigReflection.Instance.Initialize());

                while (!task.IsCompleted)
                    yield return null;

                if (task.Exception != null)
                    FARLogger.Exception(task.Exception, "Exception while setting up config");
            }
            finally
            {
                Completed = true;
            }
        }

        public static void LoadConfigs()
        {
            FARConfig.IsLoading = true;
            var load = new LoadVisitor();
            try
            {
                foreach (KeyValuePair<string, ReflectedConfig> pair in ConfigReflection.Instance.Configs)
                {
                    ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes(pair.Key);
                    if (pair.Value.Reflection.AllowMultiple)
                    {
                        var root = new ConfigNode();
                        foreach (ConfigNode configNode in nodes)
                            root.AddNode(configNode);

                        load.Node = root;
                        object instance = pair.Value.Instance;
                        pair.Value.Reflection.Load(load, ref instance);
                    }
                    else
                    {
                        if (nodes.Length == 0)
                        {
                            FARLogger.Warning($"Could not find config nodes {pair.Key}");
                            continue;
                        }

                        if (nodes.Length > 1)
                            FARLogger.Warning($"Found {nodes.Length.ToString()} {pair.Key} nodes");

                        foreach (ConfigNode node in nodes)
                        {
                            load.Node = node;
                            object instance = pair.Value.Instance;
                            pair.Value.Reflection.Load(load, ref instance);
                        }
                    }
                }

                if (!FARConfig.Debug.DumpOnLoad)
                    return;
                SaveConfigs("Custom", true, ".cfg.far");
            }
            finally
            {
                FARConfig.IsLoading = false;
            }
        }

        public static void Save()
        {
            SaveConfigs("Custom");
        }

        private static void SaveConfigs(
            string prefix,
            bool isPatch = true,
            string extension = ".cfg"
        )
        {
            var topNode = new ConfigNode();
            var node = new ConfigNode();
            var save = new SaveVisitor
            {
                IsPatch = isPatch,
                Node = node
            };
            foreach (KeyValuePair<string, ReflectedConfig> pair in ConfigReflection.Instance.Configs)
            {
                pair.Value.Reflection.Save(save, pair.Value.Instance);

                if (!save.Node.HasData)
                    continue;

                Serialization.MakeTopNode(topNode, node, pair.Key, null, isPatch);

                string path = PathUtil.Combine(KSPUtils.GameDataPath, $"{prefix}_{pair.Key}{extension}");
                FARLogger.DebugFormat("Saving {0} to {1}", pair.Key, path);
                topNode.Save(path);
                topNode.ClearData();
                node.ClearData();
            }
        }

        private IEnumerator DoLoadConfig()
        {
            Task task = Task.Factory.StartNew(LoadConfigs);

            yield return new WaitWhile(() => !task.IsCompleted);

            if (task.Exception != null)
                FARLogger.Exception(task.Exception, "Exception while loading config");

            Completed = true;
        }
    }

    public class SaveVisitor : INodeSaver
    {
        public ConfigNode Node { get; set; }
        public bool IsPatch { get; set; }

        /// <inheritdoc />
        public bool OnValue(object value, ValueReflection reflection, out object newValue)
        {
            newValue = default;
            if (value != null)
                Serialization.AddValue(Node, reflection.Name, value, reflection.ValueType, IsPatch);

            return false;
        }

        /// <inheritdoc />
        public bool OnListValue(
            int index,
            object value,
            ListValueReflection reflection,
            out object newValue
        )
        {
            newValue = default;
            if (value != null)
                Serialization.AddValue(Node, reflection.Name, value, reflection.ValueType, IsPatch, index);

            return false;
        }

        /// <inheritdoc />
        public bool OnNode(object value, NodeReflection reflection, out object newValue)
        {
            newValue = default;
            var save = new SaveVisitor
            {
                IsPatch = IsPatch,
                Node = new ConfigNode()
            };
            reflection.Save(save, value);

            if (save.Node.HasData)
                Serialization.AddNode(Node, save.Node, reflection.Id, reflection.Name, IsPatch);

            return false;
        }

        /// <inheritdoc />
        public bool OnListNode(
            int index,
            object value,
            ListValueReflection reflection,
            NodeReflection nodeReflection,
            out object newValue
        )
        {
            newValue = default;
            var save = new SaveVisitor
            {
                IsPatch = IsPatch,
                Node = new ConfigNode()
            };
            nodeReflection.Save(save, value);

            if (save.Node.HasData)
                Serialization.AddNode(Node, save.Node, nodeReflection.Id, reflection.Name, IsPatch, index);

            return false;
        }
    }

    public class LoadVisitor : INodeLoader
    {
        public ConfigNode Node { get; set; }

        /// <inheritdoc />
        public bool OnValue(ValueReflection reflection, out object newValue)
        {
            if (!Serialization.TryGetValue(Node, reflection.Name, out newValue, reflection.ValueType))
                return false;

            FARLogger.DebugFormat("Parsed {0}.{1} = {2}", Node.name, reflection.Name, newValue);
            return true;
        }

        /// <inheritdoc />
        public bool OnValueList(ListValueReflection reflection, out IList newValue)
        {
            List<string> values = Node.GetValuesList(reflection.Name);
            newValue = default;
            if (values.Count == 0)
                return false;

            newValue = new List<object>();
            for (int i = 0; i < values.Count; i++)
            {
                string str = values[i];
                if (!Serialization.TryGetValue(str, out object v, reflection.ValueType))
                    continue;

                FARLogger.DebugFormat("Parsed {0}.{1}[{3}] = {2}", Node.name, reflection.Name, v, i.ToString());
                newValue.Add(v);
            }

            return true;
        }

        /// <inheritdoc />
        public bool OnNode(object nodeObject, NodeReflection reflection, out object newValue)
        {
            ConfigNode node = string.IsNullOrEmpty(reflection.Name)
                                  ? Node.GetNode(reflection.Id)
                                  : Node.GetNode(reflection.Id, "name", reflection.Name);
            if (node != null)
            {
                var load = new LoadVisitor {Node = node};
                reflection.Load(load, ref nodeObject);
            }

            newValue = nodeObject;
            return true;
        }

        /// <inheritdoc />
        public bool OnNodeList(
            ListValueReflection reflection,
            NodeReflection nodeReflection,
            out IList newValue
        )
        {
            ConfigNode[] nodes = Node.GetNodes(reflection.NodeId);
            var load = new LoadVisitor();

            int index = 0;
            int i = 0;

            var items = new List<object>(nodes.Length);

            foreach (ConfigNode node in nodes)
            {
                if (!node.TryGetValue("index", ref i))
                    i = index;
                index++;

                load.Node = node;
                object item = nodeReflection.Load(load);
                SetItem(items, item, i);
            }

            newValue = items;
            return true;
        }

        private static void SetItem(IList list, object item, int index)
        {
            for (int i = list.Count; i <= index; i++)
                list.Add(default);

            list[index] = item;
        }
    }
}
