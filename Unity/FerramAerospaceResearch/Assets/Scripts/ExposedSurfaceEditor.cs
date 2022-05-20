using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FerramAerospaceResearch.Geometry;
using FerramAerospaceResearch.Resources;
using UnityEngine;

namespace FerramAerospaceResearch.Editor {
    [Serializable]
    public struct ColorObject {
        public Color color;
        public GameObject obj;
    }

    class ExposedSurfaceEditor : ExposedSurfaceEvaluator {
        public List<GameObject> objects = new List<GameObject>();
        private Texture2D blackTexture;

        public Vector3 lookDir = Vector3.forward;
        public Bounds bounds = new Bounds();
        public ProcessingJobType jobType;
        private RenderTexture renderTexture;

        public Shader exposedSurfaceShader;
        public ComputeShader pixelCountShader;
        public Kernel pixelCountMain;

        private void Start() {
            FARLogger.Level = LogLevel.Trace;
            FARLogger.InfoFormat("Level: {0}", FARLogger.Level);

            if (pixelCountShader != null) {
                pixelCountMain = new Kernel(pixelCountShader, pixelCountMain.name);
            }

            Initialize(exposedSurfaceShader, pixelCountShader, pixelCountMain);

            foreach (GameObject obj in objects) {
                SetupRenderers(obj, obj.GetComponentsInChildren<Renderer>(false));
            }

            blackTexture = new Texture2D(renderSize.x, renderSize.y);
            Color[] pixels = blackTexture.GetPixels();
            for (int i = 0; i < pixels.Length; ++i) pixels[i] = Color.black;
            blackTexture.SetPixels(pixels);
            blackTexture.Apply();
        }

        private void Update() {
            if (RenderPending) return;

            Render(new Request(){
                bounds=bounds,
                forward=lookDir,
                jobType=jobType,
            });

            Camera c = Camera.main;
            Transform t = c.transform;
            t.position = camera.transform.position;
            t.rotation = camera.transform.rotation;
            t.localScale = camera.transform.localScale;
            t.forward = camera.transform.forward;
            c.farClipPlane = camera.farClipPlane;
            c.orthographicSize = camera.orthographicSize;
            c.orthographic = camera.orthographic;
            c.cullingMask = camera.cullingMask;
            c.nearClipPlane = camera.nearClipPlane;
            c.clearFlags = camera.clearFlags;
            c.depthTextureMode = camera.depthTextureMode;
        }

        protected override void CompleteTextureProcessing(Result result) {
            renderTexture = result.renderTexture;
            if (!Application.isEditor) return;
            int pixels = renderTexture.width * renderTexture.height;
            double area = pixels * result.areaPerPixel;
            foreach (var pair in result.areas) {
                FARLogger.InfoFormat("{0}: {1} ({2}%)", pair.Key, pair.Value, 100 * pair.Value / area);
            }
        }

        private void OnGUI()
		{
            if (renderTexture == null) return;
            Rect sz = Screen.safeArea;
			GUI.DrawTexture(new Rect(sz.xMax - renderSize.x, sz.yMax - renderSize.y, renderSize.x, renderSize.y), blackTexture);
			GUI.DrawTexture(new Rect(sz.xMax - renderSize.x, sz.yMax - renderSize.y, renderSize.x, renderSize.y), renderTexture);
		}
    }
}
