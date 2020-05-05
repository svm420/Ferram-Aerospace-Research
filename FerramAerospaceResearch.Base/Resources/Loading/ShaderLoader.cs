using System.Collections;
using UnityEngine;

namespace FerramAerospaceResearch.Resources.Loading
{
    public class ShaderLoader : IAssetLoader<Shader>
    {
        public IEnumerator Load(IAssetRequest<Shader> assetRequest)
        {
            assetRequest.State = Progress.InProgress;

            var shader = Shader.Find(assetRequest.Url);

            if (shader == null)
            {
                FARLogger.DebugFormat("Could not find shader {0}", assetRequest.Url);
                assetRequest.State = Progress.Error;
                assetRequest.OnError();
            }
            else
            {
                FARLogger.DebugFormat("Found shader {0}", assetRequest.Url);
                assetRequest.State = Progress.Completed;
                assetRequest.OnLoad(shader);
            }

            yield break;
        }

        public string Name { get; } = "default";
    }
}
