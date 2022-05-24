using System;
using System.Collections.Generic;
using System.Reflection;
using FerramAerospaceResearch.FARGUI;
using FerramAerospaceResearch.FARPartGeometry;
using FerramAerospaceResearch.Geometry;
using FerramAerospaceResearch.Resources;
using Unity.Mathematics;
using UnityEngine;

namespace FerramAerospaceResearch.FARAeroComponents
{
    public class VehicleExposure : MonoBehaviour
    {
        private static bool supportsComputeShaders;

        public enum Device
        {
            PreferGPU,
            CPU,
            GPU,
            None,
        }

        public static readonly Device[] DeviceOptions = { Device.PreferGPU, Device.CPU, Device.GPU, Device.None };

        public readonly ObjectTagger Tagger = new();

        private readonly Dictionary<Part, List<Renderer>> cachedRenderers =
            new(ObjectReferenceEqualityComparer<Part>.Default);

        public ExposedSurface Airstream { get; private set; }
        public ExposedSurface Sun { get; private set; }
        public Device ComputeDevice { get; set; } = Device.CPU;

        public Bounds VesselBounds { get; set; }
        private Vessel vessel;

        public Vessel Vessel
        {
            get { return vessel; }
            set
            {
                if (vessel == value)
                    return;
                vessel = value;
                OnVesselEvent(value);
            }
        }

        public int2 RenderSize
        {
            get { return Airstream.renderSize; }
            set
            {
                Airstream.renderSize = value;
                Sun.renderSize = value;
            }
        }

        public Color DebugBackgroundColor
        {
            get { return Airstream.DebugBackgroundColor; }
            set
            {
                Airstream.DebugBackgroundColor = value;
                Sun.DebugBackgroundColor = value;
            }
        }

        public bool Enabled
        {
            get { return ComputeDevice != Device.None; }
        }

        public bool InAtmosphere
        {
            get { return Vessel.atmDensity > 1e-10; }
        }

        private static readonly MethodInfo partInit =
            typeof(Part).GetMethod("CreateRendererLists", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly object[] noArgs = Array.Empty<object>();
        private static readonly GUILayoutOption[] noLayoutOptions = Array.Empty<GUILayoutOption>();

        private bool displayAirstream = true;
        private bool displayArrow;
        private bool displayLabels = true;
        private bool overrideCameraShader;
        private GUIDropDown<Device> deviceSelect;

        private void Start()
        {
            supportsComputeShaders = SystemInfo.supportsComputeShaders;
            Airstream = ExposedSurface.Create(transform);
            Sun = ExposedSurface.Create(transform);

            Airstream.tagger = Tagger;
            Airstream.ArrowColor = Color.red;

            Sun.tagger = Tagger;
            Sun.ArrowColor = Color.yellow;

            GameEvents.onVesselStandardModification.Add(OnVesselEvent);
        }

        public void ResetParts<T>(T parts) where T : IEnumerable<Part>
        {
            Tagger.Reset();
            cachedRenderers.Clear();
            foreach (Part part in parts)
            {
                partInit.Invoke(part, noArgs); // make sure renderers are setup by the part
                List<Renderer> renderers = part.FindModelRenderersCached();
                renderers.RemoveAll(renderer => renderer.gameObject.layer != 0);
                Color c = Tagger.SetupRenderers(part, renderers);
                cachedRenderers.Add(part, renderers);
                part.mpb.SetColor(ShaderPropertyIds.ExposedColor, c);

                if (!overrideCameraShader)
                    continue;

                part.mpb.SetTexture(ShaderPropertyIds.ColorTex, Airstream.DebugColors);
                part.mpb.SetColor(ShaderPropertyIds._BackgroundColor, Airstream.DebugBackgroundColor);
                foreach (Renderer renderer in renderers)
                    renderer.SetPropertyBlock(part.mpb);
            }
        }

        private void UpdateRenderers()
        {
            foreach (KeyValuePair<Part, List<Renderer>> pair in cachedRenderers)
            {
                Tagger.SetupRenderers(pair.Key, pair.Value);
            }
        }

        private void OnVesselEvent(Vessel v)
        {
            if (ReferenceEquals(v, Vessel))
                ResetParts(Vessel.parts);
        }

        private void LateUpdate()
        {
            if (FlightDriver.Pause || Tagger.Count == 0)
                return;

            ExposedSurfaceEvaluator.ProcessingDevice device;
            switch (ComputeDevice)
            {
                case Device.PreferGPU:
                    device = supportsComputeShaders
                                 ? ExposedSurfaceEvaluator.ProcessingDevice.GPU
                                 : ExposedSurfaceEvaluator.ProcessingDevice.CPU;
                    break;
                case Device.CPU:
                    device = ExposedSurfaceEvaluator.ProcessingDevice.CPU;
                    break;
                case Device.GPU:
                    device = ExposedSurfaceEvaluator.ProcessingDevice.GPU;
                    break;
                case Device.None:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO: bounds seem off in KSP
            float4x4 vesselLocalToWorldMatrix = Vessel.transform.localToWorldMatrix;

            if (InAtmosphere)
            {
                Vector3 forward = Vessel.velocityD.magnitude < 0.001
                                      ? Vector3.forward
                                      : Vessel.transform.InverseTransformDirection(Vessel.srf_velocity).normalized;
                Airstream.Render(new ExposedSurfaceEvaluator.Request
                {
                    device = device,
                    forward = -forward,
                    bounds = VesselBounds,
                    toWorldMatrix = vesselLocalToWorldMatrix,
                });
            }

            Vector3 fromSun =
                (VesselBounds.center - Vessel.transform.InverseTransformPoint(FlightGlobals.Bodies[0].position))
                .normalized;
            Sun.Render(new ExposedSurfaceEvaluator.Request
            {
                device = device,
                forward = fromSun,
                bounds = VesselBounds,
                toWorldMatrix = vesselLocalToWorldMatrix,
            });
        }

        public void Display()
        {
            deviceSelect ??= new GUIDropDown<Device>(new[]
                                                     {
                                                         LocalizerExtensions.Get("FARDevicePreferGPU"),
                                                         LocalizerExtensions.Get("FARDeviceCPU"),
                                                         LocalizerExtensions.Get("FARDeviceGPU"),
                                                         LocalizerExtensions.Get("FARDeviceNone"),
                                                     },
                                                     DeviceOptions,
                                                     DeviceOptions.IndexOf(ComputeDevice));
            deviceSelect.GUIDropDownDisplay(noLayoutOptions);
            ComputeDevice = deviceSelect.ActiveSelection;

            GUILayout.BeginHorizontal();
            int2 size = RenderSize;
            size.x = GUIUtils.TextEntryForInt(LocalizerExtensions.Get("FARExposureWidthLabel"), 100, size.x);
            size.y = GUIUtils.TextEntryForInt(LocalizerExtensions.Get("FARExposureHeightLabel"), 100, size.y);
            RenderSize = size;
            GUILayout.EndHorizontal();

            // TODO: background color picker

            GUI.enabled = Enabled && InAtmosphere;
            if (!InAtmosphere)
                displayAirstream = false; // no airstream in space
            displayAirstream = GUILayout.Toggle(displayAirstream,
                                                displayAirstream
                                                    ? LocalizerExtensions.Get("FARExposureAirstreamLabel")
                                                    : LocalizerExtensions.Get("FARExposureSunLabel"));

            GUI.enabled = Enabled;
            displayArrow = GUILayout.Toggle(displayArrow, LocalizerExtensions.Get("FARExposureArrowLabel"));
            Airstream.DisplayArrow = displayArrow && InAtmosphere;
            Sun.DisplayArrow = displayArrow;

            bool overriden = overrideCameraShader;
            overrideCameraShader =
                GUILayout.Toggle(overrideCameraShader, LocalizerExtensions.Get("FARExposureShaderLabel"));
            if (overriden != overrideCameraShader)
            {
                if (overrideCameraShader)
                {
                    Camera.main.SetReplacementShader(FARAssets.Instance.Shaders.ExposedSurfaceCamera, null);
                    ResetParts(Vessel.parts);
                }
                else
                    Camera.main.ResetReplacementShader();
            }

            displayLabels = GUILayout.Toggle(displayLabels, LocalizerExtensions.Get("FARExposureShowLabelsLabel"));
            GUI.enabled = true;

            if (!Enabled)
                return;

            if (displayAirstream)
                Airstream.DrawDebugImage(250, displayLabels);
            else
                Sun.DrawDebugImage(250, displayLabels);
        }

        private void OnDestroy()
        {
            GameEvents.onVesselStandardModification.Remove(OnVesselEvent);
            Airstream.CancelPendingJobs();
            Sun.CancelPendingJobs();
            Tagger.Dispose();
        }
    }
}
