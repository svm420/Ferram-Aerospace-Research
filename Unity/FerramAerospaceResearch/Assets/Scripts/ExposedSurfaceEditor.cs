using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FerramAerospaceResearch.Geometry.Exposure;
using FerramAerospaceResearch.Resources;
using Unity.Mathematics;
using UnityEngine;
using Renderer = UnityEngine.Renderer;

namespace FerramAerospaceResearch.Editor
{
    public class ExposedSurfaceEditor : MonoBehaviour
    {
        [Serializable]
        public struct ObjectColor
        {
            public GameObject go;
            public Color color;
        }

        public List<ObjectColor> objects = new List<ObjectColor>();

        public Vector3 lookDir = Vector3.forward;
        public Bounds bounds;
        public ProcessingDevice device;
        private RenderTexture renderTexture;

        public Vector2Int renderSize = new Vector2Int(512, 512);
        public Shader exposedSurfaceShader;
        public ComputeShader pixelCountShader;
        public Kernel pixelCountMain;

        public Shader debugShader;
        public Color32 debugBackground = Color.black;
        private List<Color> debugColorList = new List<Color>();
        private Texture2D debugColors;

        private Renderer<GameObject> exposureRenderer;
        private readonly List<RenderRequest> requests = new List<RenderRequest>();
        private Debugger debugger;

        private Material exposedMaterial;
        private Material debugMaterial;

        private static readonly string screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");

        private void Start()
        {
            FARLogger.Level = LogLevel.Trace;

            if (pixelCountShader != null)
                pixelCountMain = new Kernel(pixelCountShader, pixelCountMain.name);

            exposedMaterial = new Material(exposedSurfaceShader);
            debugMaterial = new Material(debugShader);

            exposureRenderer = new Renderer<GameObject>()
            {
                Bounds = bounds,
                Device = device,
                Material = exposedMaterial,
                PixelCountKernel = pixelCountMain,
                PixelCountShader = pixelCountShader,
                RenderSize = new int2(renderSize.x, renderSize.y),
            };

            debugger = new Debugger()
            {
                enabled = true,
                Material = debugMaterial,
            };

            debugColorList.Capacity = objects.Count;
            foreach (ObjectColor obj in objects)
            {
                exposureRenderer.SetupRenderers(obj.go, obj.go.GetComponentsInChildren<Renderer>(false));
                debugColorList.Add(obj.color);
            }

            debugColors = new Texture2D(debugColorList.Count, 1)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };
            debugColors.SetPixels(debugColorList.ToArray());
            debugColors.Apply();

            requests.Add(new RenderRequest()
            {
                debugger = debugger,
                result = new RenderResult<GameObject>() { renderer = exposureRenderer },
                callback = OnRequestComplete,
                userData = this
            });

            OnValidate();
        }

        private void Update()
        {
            RenderRequest r = requests[0];
            r.lookDir = lookDir;

            requests[0] = r;
            exposureRenderer.RenderSize = new int2(renderSize.x, renderSize.y);
            exposureRenderer.Render(requests, float4x4.identity);
        }

        private static void OnRequestComplete(RenderResult result, object _)
        {
            OnRequestComplete(result as RenderResult<GameObject>);
        }

        private static void OnRequestComplete(RenderResult<GameObject> result)
        {
            if (!Application.isEditor)
                return;
            foreach (KeyValuePair<GameObject, double> pair in result)
            {
                FARLogger.InfoFormat("{0}: {1}", pair.Key, pair.Value);
            }
        }

        private Rect windowRect;

        private void OnGUI()
        {
            if (debugger.texture == null)
                return;

            windowRect = GUILayout.Window(0, windowRect, OnWindow, "Debug Display");
        }

        private void OnWindow(int _)
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button("Save to PNG"))
                SavePNG(false);
            if (GUILayout.Button("Save to raw PNG"))
                SavePNG(true);
            debugger.DrawTexture(renderSize.x,renderSize.y);
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void SavePNG(bool raw)
        {
            if (!Directory.Exists(screenshotDir))
                Directory.CreateDirectory(screenshotDir);
            string path = Path.Combine(screenshotDir, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".png");
            debugger.SaveToPNG(path, raw);
        }

        private void OnDestroy()
        {
            exposureRenderer.Dispose();
            debugger.Dispose();
            requests.Clear();
        }

        private void OnValidate()
        {
            if (debugColors == null || debugger is null)
                return;

            if (debugColors.width != objects.Count)
            {
                Destroy(debugColors);
                debugColors = new Texture2D(objects.Count, 1)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Repeat
                };
            }

            debugColors.SetPixels(objects.Select(oc => oc.color).ToArray());
            debugColors.Apply();

            debugger.BackgroundColor = debugBackground;
            debugger.ObjectColors = debugColors;
        }
    }
}
