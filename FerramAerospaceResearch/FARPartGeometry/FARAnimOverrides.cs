using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace FerramAerospaceResearch
{
    public static class FARAnimOverrides
    {
        private static Dictionary<string, string> animOverrides;

        public static void LoadAnimOverrides()
        {
            animOverrides = new Dictionary<string, string>();
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("FARAnimOverride");
            for (int i = 0; i < nodes.Length; i++)
            {
                string moduleName = nodes[i].GetValue("moduleName");
                string animNameField = nodes[i].GetValue("animNameField");
                if (moduleName != null && animNameField != null && moduleName != string.Empty && animNameField != string.Empty)
                    animOverrides.Add(moduleName, animNameField);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static bool OverrideExists(string moduleName)
        {
            return animOverrides.ContainsKey(moduleName);
        }

        public static string FieldNameForModule(string moduleName)
        {
            try
            {
                return animOverrides[moduleName];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
    }
}
