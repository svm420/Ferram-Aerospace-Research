using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FerramAerospaceResearch.FARUtils;
using UnityEngine;

namespace FerramAerospaceResearch
{
    // this is only required to make sure IConfigProvider exists before FARConfig is first accessed
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class FARConfigSetup : MonoBehaviour
    {
        private List<KeyValuePair<ConfigParserAttribute, Type>> parserTypes;

        private void Start()
        {
            StartCoroutine(InstantiateParsers());
        }

        private void CollectTypes(IEnumerable<AssemblyLoader.LoadedAssembly> loadedAssemblies)
        {
            parserTypes =
                new List<KeyValuePair<ConfigParserAttribute, Type>>(AssemblyUtils
                                                                        .FindAttribute<ConfigParserAttribute
                                                                        >(loadedAssemblies));
        }

        private IEnumerator InstantiateParsers()
        {
            // iterating over every type in every assembly may be expensive so do it asynchronously
            Task task =
                Task.Factory.StartNew(() =>
                                          CollectTypes(new List<AssemblyLoader.LoadedAssembly>(AssemblyLoader
                                                                                                   .loadedAssemblies)));
            yield return null;

            // wait until all the types are collected
            while (!task.IsCompleted)
                yield return null;

            if (task.Exception != null)
            {
                FARLogger.Error("Collecting types failed with exceptions:");
                foreach (Exception ex in task.Exception.InnerExceptions)
                    FARLogger.Error($"  {ex.ToString()}");
                yield break;
            }

            var provider = new ConfigProvider();
            foreach (KeyValuePair<ConfigParserAttribute, Type> pair in parserTypes)
            {
                object parserInstance = Activator.CreateInstance(pair.Value);
                if (!(parserInstance is FARConfigParser instance))
                {
                    FARLogger.Warning($"Found type with ConfigParserAttribute '{pair.Key.Name}' with the wrong type {parserInstance.GetType().ToString()}");
                    continue;
                }

                provider.Register(pair.Key.Name, instance);

                // distribute instantiations over multiple frames
                yield return null;
            }

            FARConfig.Provider = provider;
        }

        public class ConfigProvider : FARConfigProvider
        {
            public override IConfigNode[] LoadConfigs(string name)
            {
                ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes(name);

                if (nodes.Length == 0)
                {
                    FARLogger.Error($"Could not find {name} node in GameDatabase");
                    return new IConfigNode[0];
                }

                if (nodes.Length > 1)
                    FARLogger.Info($"Found {nodes.Length.ToString()} {name} nodes");

                return ConfigNodeWrapper.Wrap(nodes);
            }

            public override IConfigNode CreateNode()
            {
                return new ConfigNodeWrapper();
            }

            public override IConfigNode CreateNode(string name)
            {
                return new ConfigNodeWrapper(name);
            }

            public override IConfigNode CreateNode(string name, string vcomment)
            {
                return new ConfigNodeWrapper(name, vcomment);
            }

            public void Register(string name, FARConfigParser parser)
            {
                if (Parsers.TryGetValue(name, out FARConfigParser configParser))
                {
                    FARLogger.Warning(configParser.GetType() == parser.GetType()
                                          ? $"Parser '{name}' is already registered"
                                          : $"Parser '{name}' is registered with different type {configParser.GetType().ToString()}");
                    return;
                }

                Parsers.Add(name, parser);
                FARLogger.Debug($"Registered config parser '{name}'");
            }
        }
    }
}
