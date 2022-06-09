using System;
using System.Collections.Generic;
using System.Reflection;
using FerramAerospaceResearch.FARGUI;
using FerramAerospaceResearch.FARPartGeometry;
using FerramAerospaceResearch.Geometry.Exposure;
using FerramAerospaceResearch.Resources;
using Unity.Mathematics;
using UnityEngine;
using Renderer = UnityEngine.Renderer;

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

        private readonly Renderer<Part> exposureRenderer = new();
        private BoundsRenderer boundsRenderer;

        private readonly Dictionary<Part, List<Renderer>> cachedRenderers =
            new(ObjectReferenceEqualityComparer<Part>.Default);

        private Device computeDevice = Device.PreferGPU;

        public Device ComputeDevice
        {
            get { return computeDevice; }
            set
            {
                computeDevice = value;
                switch (value)
                {
                    case Device.PreferGPU:
                        exposureRenderer.Device = supportsComputeShaders ? ProcessingDevice.GPU : ProcessingDevice.CPU;
                        break;
                    case Device.CPU:
                        exposureRenderer.Device = ProcessingDevice.CPU;
                        break;
                    case Device.GPU:
                        exposureRenderer.Device = ProcessingDevice.GPU;
                        break;
                    case Device.None:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private RenderRequest airstreamRequest;
        private RenderRequest sunRequest;
        private readonly List<RenderRequest> requests = new();

        private ExposureDebugger AirstreamDebugger
        {
            get { return (ExposureDebugger)airstreamRequest.debugger; }
        }

        private ExposureDebugger SunDebugger
        {
            get { return (ExposureDebugger)sunRequest.debugger; }
        }

        public Bounds VesselBounds
        {
            get { return exposureRenderer.Bounds; }
            set
            {
                exposureRenderer.Bounds = value;
                boundsRenderer.VesselBounds = value;
            }
        }

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
            get { return exposureRenderer.RenderSize; }
            set { exposureRenderer.RenderSize = value; }
        }

        private Color debugBackgroundColor = Color.black;

        public Color DebugBackgroundColor
        {
            get { return debugBackgroundColor; }
            set
            {
                debugBackgroundColor = value;
                airstreamRequest.debugger.BackgroundColor = value;
                sunRequest.debugger.BackgroundColor = value;
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

        private void Awake()
        {
            supportsComputeShaders = SystemInfo.supportsComputeShaders;
            deviceSelect = new GUIDropDown<Device>(new[]
                                                   {
                                                       LocalizerExtensions.Get("FARDevicePreferGPU"),
                                                       LocalizerExtensions.Get("FARDeviceCPU"),
                                                       LocalizerExtensions.Get("FARDeviceGPU"),
                                                       LocalizerExtensions.Get("FARDeviceNone"),
                                                   },
                                                   DeviceOptions,
                                                   DeviceOptions.IndexOf(ComputeDevice));

            ComputeDevice = computeDevice;
            exposureRenderer.Material = FARAssets.Instance.Shaders.ExposedSurface;
            exposureRenderer.RenderSize = RenderSize;
            exposureRenderer.PixelCountKernel = FARAssets.Instance.ComputeShaders.CountPixels.Kernel;
            exposureRenderer.PixelCountShader = FARAssets.Instance.ComputeShaders.CountPixels;

            airstreamRequest = new RenderRequest
            {
                debugger = new ExposureDebugger
                {
                    BackgroundColor = debugBackgroundColor,
                    enabled = false,
                    Material = FARAssets.Instance.Shaders.ExposedSurfaceDebug,
                    ObjectColors = FARConfig.Voxelization.ColorMapTexture(),
                    ArrowColor = Color.red,
                    DisplayArrow = false
                },
                result = new RenderResult<Part> { renderer = exposureRenderer },
                userData = this
            };

            sunRequest = new RenderRequest
            {
                debugger = new ExposureDebugger
                {
                    BackgroundColor = debugBackgroundColor,
                    enabled = false,
                    Material = airstreamRequest.debugger.Material,
                    ObjectColors = airstreamRequest.debugger.ObjectColors,
                    ArrowColor = Color.yellow,
                    DisplayArrow = false
                },
                result = new RenderResult<Part> { renderer = exposureRenderer },
                userData = this
            };

            boundsRenderer = BoundsRenderer.Get();
        }

        private void Start()
        {
            GameEvents.onVesselStandardModification.Add(OnVesselEvent);
            GameEvents.onVesselChange.Add(OnActiveVesselEvent);

            enabled = Vessel == FlightGlobals.ActiveVessel;
        }

        public void ResetParts<T>(T parts) where T : IEnumerable<Part>
        {
            exposureRenderer.Reset();
            cachedRenderers.Clear();
            foreach (Part part in parts)
            {
                partInit.Invoke(part, noArgs); // make sure renderers are setup by the part
                List<Renderer> renderers = part.FindModelRenderersCached();
                renderers.RemoveAll(renderer => renderer.gameObject.layer != 0);
                exposureRenderer.SetupRenderers(part, renderers, part.mpb);
                cachedRenderers.Add(part, renderers);
            }
        }

        private void SetupPartsForDebugCamera()
        {
            foreach (KeyValuePair<Part, List<Renderer>> pair in cachedRenderers)
            {
                pair.Key.mpb.SetTexture(ShaderPropertyIds._ColorTex, airstreamRequest.debugger.ObjectColors);
                pair.Key.mpb.SetColor(ShaderPropertyIds._BackgroundColor, debugBackgroundColor);

                foreach (Renderer renderer in pair.Value)
                    renderer.SetPropertyBlock(pair.Key.mpb);
            }
        }

        private void UpdateRenderers()
        {
            foreach (KeyValuePair<Part, List<Renderer>> pair in cachedRenderers)
                exposureRenderer.SetupRenderers(pair.Key, pair.Value, pair.Key.mpb);
        }

        private void OnVesselEvent(Vessel v)
        {
            if (v == Vessel)
                ResetParts(Vessel.parts);
        }

        private void OnActiveVesselEvent(Vessel v)
        {
            enabled = v == Vessel;
        }

        private void LateUpdate()
        {
            if (FlightDriver.Pause || exposureRenderer.Count == 0 || !Enabled)
                return;

            // TODO: bounds seem off in KSP
            float4x4 vesselLocalToWorldMatrix = Vessel.transform.localToWorldMatrix;
            requests.Clear();

            airstreamRequest.debugger.enabled = false;
            sunRequest.debugger.enabled = false;

            if (InAtmosphere)
            {
                Vector3 forward = Vessel.velocityD.magnitude < 0.001
                                      ? Vector3.forward
                                      : Vessel.transform.InverseTransformDirection(Vessel.srf_velocity).normalized;
                requests.Add(airstreamRequest with { lookDir = -forward });
            }

            Vector3 fromSun =
                (VesselBounds.center - Vessel.transform.InverseTransformPoint(FlightGlobals.Bodies[0].position))
                .normalized;
            requests.Add(sunRequest with { lookDir = fromSun });

            exposureRenderer.Render(requests, vesselLocalToWorldMatrix);
        }

        public bool Display()
        {
            airstreamRequest.debugger.enabled = true;
            sunRequest.debugger.enabled = true;

            deviceSelect.GUIDropDownDisplay(noLayoutOptions);
            ComputeDevice = deviceSelect.ActiveSelection;

            GUILayout.BeginHorizontal();
            int2 size = RenderSize;
            size.x = GUIUtils.TextEntryForInt(LocalizerExtensions.Get("FARExposureWidthLabel"), 50, size.x);
            size.y = GUIUtils.TextEntryForInt(LocalizerExtensions.Get("FARExposureHeightLabel"), 50, size.y);
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
            AirstreamDebugger.DisplayArrow = displayArrow && InAtmosphere;
            SunDebugger.DisplayArrow = displayArrow;

            boundsRenderer.enabled =
                GUILayout.Toggle(boundsRenderer.enabled, LocalizerExtensions.Get("FARDrawBoundsLabel"));

            bool overridden = overrideCameraShader;
            overrideCameraShader =
                GUILayout.Toggle(overrideCameraShader, LocalizerExtensions.Get("FARExposureShaderLabel"));
            if (overridden != overrideCameraShader)
            {
                if (overrideCameraShader)
                {
                    SetupPartsForDebugCamera();
                    Camera.main.SetReplacementShader(FARAssets.Instance.Shaders.ExposedSurfaceCamera, null);
                }
                else
                    Camera.main.ResetReplacementShader();
            }

            bool previous = displayLabels;
            displayLabels = GUILayout.Toggle(displayLabels, LocalizerExtensions.Get("FARExposureShowLabelsLabel"));
            GUI.enabled = true;

            if (!Enabled)
                return previous != displayLabels;

            if (displayAirstream)
                AirstreamDebugger.Display(250, displayLabels);
            else
                SunDebugger.Display(250, displayLabels);

            return previous != displayLabels;
        }

        private void OnDestroy()
        {
            GameEvents.onVesselStandardModification.Remove(OnVesselEvent);
            GameEvents.onVesselChange.Remove(OnActiveVesselEvent);

            exposureRenderer.Dispose();
            requests.Clear();

            airstreamRequest.debugger.Dispose();
            airstreamRequest = default;

            sunRequest.debugger.Dispose();
            sunRequest = default;
        }
    }
}
