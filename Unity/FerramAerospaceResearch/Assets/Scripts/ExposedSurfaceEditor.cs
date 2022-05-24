using System.Collections.Generic;
using FerramAerospaceResearch.Geometry;
using FerramAerospaceResearch.Resources;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FerramAerospaceResearch.Editor {
    class ExposedSurfaceEditor : ExposedSurfaceEvaluator {
        public List<GameObject> objects = new List<GameObject>();

        public Vector3 lookDir = Vector3.forward;
        public Bounds bounds;
        public ProcessingDevice device;
        private RenderTexture renderTexture;

        public Shader exposedSurfaceShader;
        public ComputeShader pixelCountShader;
        public Kernel pixelCountMain;
        public Material debugMaterial;

        private void Start() {
            FARLogger.Level = LogLevel.Trace;
            FARLogger.InfoFormat("Level: {0}", FARLogger.Level);

            tagger = new ObjectTagger();

            if (pixelCountShader != null) {
                pixelCountMain = new Kernel(pixelCountShader, pixelCountMain.name);
            }

            Initialize(exposedSurfaceShader, pixelCountShader, pixelCountMain);

            foreach (GameObject obj in objects) {
                tagger.SetupRenderers(obj, obj.GetComponentsInChildren<Renderer>(false));
            }
        }

        private void Update() {
            Render(new Request(){
                bounds=bounds,
                forward=lookDir,
                device=device,
            });

            Camera c = Camera.main;
            if (c == null)
                return;
            c.CopyFrom(camera);
            c.targetTexture = null;
            c.ResetReplacementShader();
        }

        protected override void CompleteTextureProcessing(Result result) {
            renderTexture = result.renderTexture;
            if (!Application.isEditor) return;
            int pixels = renderTexture.width * renderTexture.height;
            double area = pixels * result.areaPerPixel;
            foreach (KeyValuePair<Object, double> pair in result.areas) {
                FARLogger.InfoFormat("{0}: {1} ({2}%)", pair.Key, pair.Value, 100 * pair.Value / area);
            }
        }

        private void OnGUI()
		{
            if (renderTexture == null) return;
            Rect sz = Screen.safeArea;
            var render = new Rect(sz.xMax - renderSize.x, sz.yMax - renderSize.y, renderSize.x, renderSize.y);
            if (debugMaterial != null)
            {
                if (Event.current.type != EventType.Repaint)
                    return;
                Graphics.DrawTexture(render, renderTexture, debugMaterial);
            } else
			    GUI.DrawTexture(render, renderTexture);
		}

        protected override void OnDestroy()
        {
            base.OnDestroy();
            tagger.Dispose();
        }
    }
}
