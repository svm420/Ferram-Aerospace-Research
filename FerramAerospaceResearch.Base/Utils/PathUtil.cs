using System;
using System.IO;
using FerramAerospaceResearch.Reflection;

namespace FerramAerospaceResearch
{
    public static class PathUtil
    {
        public const string ModDirectoryName = "FerramAerospaceResearch";

        public static string PluginsDir { get; } =
            Path.GetFullPath(Path.Combine(ReflectionUtils.ExecutingAssembly.Location, ".."));

        public static string RootDir { get; } = Path.GetFullPath(Path.Combine(PluginsDir, ".."));
        public static string AssetsDir { get; } = Path.Combine(RootDir, "Assets");
        public static string TexturesDir { get; } = Path.Combine(RootDir, "Textures");
        public static string ParentDir { get; } = Path.GetFullPath(Path.Combine(RootDir, ".."));
        public static string PParentDir { get; } = Path.GetFullPath(Path.Combine(ParentDir, ".."));

        public static string Combine(string path, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Path.Combine(path, filename);
        }

        public static string Combine(string path, string dir1, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Path.Combine(path, dir1, filename);
        }

        public static string Combine(string path, string dir1, string dir2, string filename)
        {
            return Path.IsPathRooted(filename) ? filename : Path.Combine(path, dir1, dir2, filename);
        }

        public static string Combine(params string[] paths)
        {
            return Path.IsPathRooted(paths[paths.Length - 1]) ? paths[paths.Length - 1] : Path.Combine(paths);
        }

        public static Func<string, string> CombineDelegate(string root)
        {
            return s => Combine(root, s);
        }

        public static Func<string, string, string> CombineDelegate2(string root)
        {
            return (d, s) => Combine(root, d, s);
        }

        public static Func<string, string, string, string> CombineDelegate3(string root)
        {
            return (d1, d2, s) => Combine(root, d1, d2, s);
        }
    }
}
