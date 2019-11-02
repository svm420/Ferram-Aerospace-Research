using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FerramAerospaceResearch.FARUtils;

namespace FerramAerospaceResearch
{
    // this is only required to make sure IConfigProvider exists before FARConfig is first accessed
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class FARConfigSetup : MonoBehaviour
    {
        private void Start()
        {
            var provider = new ConfigProvider(Path.GetFullPath(KSPUtil.ApplicationRootPath));

            foreach (KeyValuePair<ConfigParserAttribute, Type> pair in AssemblyUtils
                .FindAttribute<ConfigParserAttribute>())
            {
                if (Activator.CreateInstance(pair.Value) is FARConfigParser instance)
                    provider.Register(pair.Key.Name, instance);
            }

            FARConfig.Provider = provider;
        }

        public class ConfigProvider : FARConfigProvider
        {
            public ConfigProvider(string path)
            {
                KSPRootPath = path;
            }

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

            public override string KSPRootPath { get; }

            public void Register(string name, FARConfigParser parser)
            {
                if (Parsers.TryGetValue(name, out FARConfigParser configParser))
                {
                    if (configParser.GetType() == parser.GetType())
                    {
                        FARLogger.Warning($"Parser '{name}' is already registered");
                        return;
                    }

                    FARLogger.Warning($"Parser '{name}' is registered with different type {configParser.GetType().ToString()}");
                    return;
                }

                Parsers.Add(name, parser);
                FARLogger.Debug($"Registered config parser '{name}'");
            }
        }
    }
}
