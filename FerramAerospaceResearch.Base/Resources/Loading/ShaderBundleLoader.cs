using System;
using FerramAerospaceResearch.Threading;
using UnityEngine;

namespace FerramAerospaceResearch.Resources.Loading
{
    public class ShaderBundleLoader : AssetBundleLoader<Shader>
    {
        private readonly Observable<string> pathValue;

        public ShaderBundleLoader()
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (Application.platform)
            {
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.WindowsPlayer
                    when SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL", StringComparison.Ordinal):
                    FARLogger.Info("Shaders will be loaded from Linux bundle");
                    //For OpenGL users on Windows we load the Linux shaders to fix OpenGL issues
                    pathValue = FARConfig.Shaders.BundleLinux;
                    break;
                case RuntimePlatform.WindowsPlayer:
                    FARLogger.Info("Shaders will be loaded from Windows bundle");
                    pathValue = FARConfig.Shaders.BundleWindows;
                    break;
                case RuntimePlatform.OSXPlayer:
                    FARLogger.Info("Shaders will be loaded from MacOSX bundle");
                    pathValue = FARConfig.Shaders.BundleMac;
                    break;
                default:
                    // Should never reach this
                    FARLogger.Error($"Invalid runtime platform {Application.platform}");
                    break;
            }

            Url = pathValue.Value;
            pathValue.OnValueChanged += UpdateUrl;
        }

        private void UpdateUrl(string url)
        {
            Url = url;
            LoadAssets();
        }

        public void LoadAssets()
        {
            FARLogger.Info("Loading shader bundle");
            MainThread.StartCoroutine(Load);
        }

        public void Unsubscribe()
        {
            pathValue.OnValueChanged -= UpdateUrl;
        }
    }
}
