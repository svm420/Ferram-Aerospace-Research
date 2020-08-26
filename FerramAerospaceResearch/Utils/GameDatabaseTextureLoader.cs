using System;
using System.Collections;
using FerramAerospaceResearch.Resources.Loading;
using UnityEngine;

namespace FerramAerospaceResearch
{
    public class GameDatabaseTextureLoader : IAssetLoader<Texture2D>
    {
        public IEnumerator Load(IAssetRequest<Texture2D> assetRequest)
        {
            if (!GameDatabase.Instance.IsReady())
            {
                FARLogger.Warning("Trying to load textures before GameDatabase has been loaded");
                assetRequest.State = Progress.Error;
                assetRequest.OnError();
                yield break;
            }

            assetRequest.State = Progress.InProgress;
            try
            {
                FARLogger.DebugFormat("Getting texture {0} from GameDatabase", assetRequest.Url);
                Texture2D texture = GameDatabase.Instance.GetTexture(assetRequest.Url, false);
                assetRequest.State = Progress.Completed;
                assetRequest.OnLoad(texture);
            }
            catch (Exception e)
            {
                FARLogger.Exception(e, $"While loading texture {assetRequest.Url} from game database:");
                assetRequest.State = Progress.Error;
                assetRequest.OnError();
            }
        }

        public string Name { get; } = "GameDatabase";
    }
}
