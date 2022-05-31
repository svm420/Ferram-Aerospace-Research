using System.Collections.Generic;
using FerramAerospaceResearch.Geometry;
using FerramAerospaceResearch.Resources;
using KSP.Localization;
using UnityEngine;

namespace FerramAerospaceResearch.FARPartGeometry
{
    public class ExposedSurface : ExposedSurfaceEvaluator
    {
        private RenderTexture lastImage;
        private Material debugMaterial;

        public Dictionary<Object, double> MostRecentAreas { get; private set; }
        public Color DebugBackgroundColor { get; set; } = Color.black;
        private Texture2D debugColors;

        public Texture2D DebugColors
        {
            get { return debugColors ??= FARConfig.Voxelization.ColorMapTexture(); }
        }

        private Vector2 labelScrollPosition;
        private ArrowPointer debugArrow;

        public bool DisplayArrow
        {
            get { return debugArrow.gameObject.activeSelf; }
            set { debugArrow.gameObject.SetActive(value); }
        }

        public Color ArrowColor
        {
            get { return debugArrow.Color; }
            set { debugArrow.Color = value; }
        }

        public static ExposedSurface Create(Transform parent = null)
        {
            FARComputeShaderCache.ComputeShaderAssetRequest compute = FARAssets.Instance.ComputeShaders.CountPixels;
            return ExposedSurfaceEvaluator.Create<ExposedSurface>(shader: FARAssets.Instance.Shaders.ExposedSurface,
                                                                  pixelCount: compute,
                                                                  main: compute.Kernel,
                                                                  parent: parent);
        }

        protected override void Initialize(Shader shader = null, ComputeShader pixelCount = null, Kernel? main = null)
        {
            base.Initialize(shader, pixelCount, main);
            debugArrow = ArrowPointer.Create(transform, Vector3.zero, Vector3.forward, 10f, Color.red, false);
            DisplayArrow = false;
        }

        protected override void CompleteTextureProcessing(Result result)
        {
            lastImage = result.renderTexture;
            MostRecentAreas = result.areas;
        }

        protected override void OnJobSubmitted(RenderJob job)
        {
            if (!DisplayArrow)
                return;
            Transform tr = debugArrow.transform;
            tr.position = job.position;
            tr.forward = job.forward;
        }

        public void DrawDebugImage(float width, bool partLabels = false)
        {
            if (lastImage == null)
                return;

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            Rect texRect = GUILayoutUtility.GetRect(width, width * lastImage.height / lastImage.width);

            // Graphics.DrawTexture allows using custom materials but needs to check current event is Repaint
            if (Event.current.type == EventType.Repaint)
            {
                debugMaterial ??= Instantiate(FARAssets.Instance.Shaders.ExposedSurfaceDebug.Material);
                debugMaterial.SetColor(ShaderPropertyIds._BackgroundColor, DebugBackgroundColor);
                debugMaterial.SetTexture(ShaderPropertyIds._ColorTex, DebugColors);

                // expects rect in screen coordinates
                Graphics.DrawTexture(texRect, lastImage, debugMaterial);
            }

            if (partLabels)
            {
                labelScrollPosition = GUILayout.BeginScrollView(labelScrollPosition, GUILayout.Height(250));
                string m2 = Localizer.Format("FARUnitMSq");
                foreach (KeyValuePair<Object, double> pair in MostRecentAreas)
                {
                    if (pair.Key is not Part p)
                        continue;
                    GUILayout.Label($"{p.partInfo.name}: {pair.Value:F3} {m2}");
                }

                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }
    }
}
