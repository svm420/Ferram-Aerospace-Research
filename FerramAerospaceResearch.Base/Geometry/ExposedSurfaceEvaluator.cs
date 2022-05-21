using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using FerramAerospaceResearch.Resources;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace FerramAerospaceResearch.Geometry
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ColorUintConverter
    {
        [FieldOffset(0)] private uint u;
        [FieldOffset(0)] private Color32 c;

        public static Color32 AsColor(uint value)
        {
            return new ColorUintConverter { u = value }.c;
        }

        public static uint AsUint(Color32 color)
        {
            return new ColorUintConverter { c = color }.u;
        }
    }

    public class ObjectTagger : IDisposable, IReadOnlyDictionary<Object, int>
    {
        private const int TagLength = 10;
        private const uint TagMask = (1 << TagLength) - 1;

        public uint Tag { get; private set; }
        private static readonly HashSet<uint> usedTags = new();
        private static readonly Random random = new();
        private readonly Dictionary<Object, int> objectIds = new(ObjectReferenceEqualityComparer<Object>.Default);
        private MaterialPropertyBlock block;

        public Dictionary<Object, int>.KeyCollection Keys
        {
            get { return objectIds.Keys; }
        }

        public Dictionary<Object, int>.ValueCollection Values
        {
            get { return objectIds.Values; }
        }

        private void SetUniqueTag()
        {
            do
            {
                Tag = (uint)random.Next(1, 1 << TagLength); // 0 tag would coincide with empty pixels so don't use it
            } while (usedTags.Contains(Tag));

            usedTags.Add(Tag);
        }

        public ObjectTagger()
        {
            SetUniqueTag();
        }

        public static uint GetTag(uint value)
        {
            return value & TagMask;
        }

        public static int GetIndex(uint value)
        {
            return (int)(value >> TagLength);
        }

        public uint Encode(int index)
        {
            return ((uint)(index) << TagLength) | Tag;
        }

        public void SetupRenderers<T>(Object obj, T renderers, MaterialPropertyBlock propertyBlock = null)
            where T : IEnumerable<Renderer>
        {
            foreach (Renderer renderer in renderers)
                SetupRenderer(obj, renderer, propertyBlock);
        }

        public void Reset(bool newTag = false)
        {
            objectIds.Clear();
            if (!newTag)
                return;
            usedTags.Remove(Tag);
            SetUniqueTag();
        }

        public void SetupRenderer(Object obj, Renderer renderer, MaterialPropertyBlock propertyBlock = null)
        {
            // unique per object, stability doesn't matter as renderers will have to be rebuilt when anything changes
            if (!objectIds.TryGetValue(obj, out int index))
            {
                index = objectIds.Count;
                objectIds.Add(obj, index);
            }

            if (propertyBlock == null)
            {
                block ??= new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block); // always overwrites block in any case
                propertyBlock = block;
            }

            uint id = Encode(index);
            Color c = ColorUintConverter.AsColor(id);
            propertyBlock.SetColor(ShaderPropertyIds.ExposedColor, c);
            FARLogger.DebugFormat("{0} ({1}): {2}", obj, id, c);
            renderer.SetPropertyBlock(propertyBlock);
        }

        public Dictionary<Object, int>.Enumerator GetEnumerator()
        {
            return objectIds.GetEnumerator();
        }

        IEnumerator<KeyValuePair<Object, int>> IEnumerable<KeyValuePair<Object, int>>.GetEnumerator()
        {
            return objectIds.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)objectIds).GetEnumerator();
        }

        public int Count
        {
            get { return objectIds.Count; }
        }

        public bool ContainsKey(Object key)
        {
            return objectIds.ContainsKey(key);
        }

        public bool TryGetValue(Object key, out int value)
        {
            return objectIds.TryGetValue(key, out value);
        }

        public int this[Object key]
        {
            get { return objectIds[key]; }
        }

        IEnumerable<Object> IReadOnlyDictionary<Object, int>.Keys
        {
            get { return objectIds.Keys; }
        }

        IEnumerable<int> IReadOnlyDictionary<Object, int>.Values
        {
            get { return objectIds.Values; }
        }

        public void Dispose()
        {
            usedTags.Remove(Tag);
        }
    }

    public abstract class ExposedSurfaceEvaluator : MonoBehaviour
    {
        private static bool computeWarningIssued;
        public ObjectTagger Tagger;
        public Camera camera;
        public Vector2Int renderSize = new()
        {
            x = 512,
            y = 512
        };

        public int depthBits = 24;

        public Shader Shader
        {
            set
            {
                if (value == null)
                    camera.ResetReplacementShader();
                else
                    camera.SetReplacementShader(value, string.Empty);
            }
        }

        public Kernel MainKernel { get; private set; }
        public ComputeShader PixelCountShader { get; private set; }

        public void SetComputeShader(ComputeShader shader, Kernel? main = default)
        {
            PixelCountShader = shader;
            MainKernel = main ?? new Kernel(shader, "CountPixelsMain");
        }

        public enum ProcessingDevice
        {
            CPU,
            GPU,
        }

        public struct Request
        {
            public Bounds bounds;
            public Vector3 forward;
            public Action callback;
            public ProcessingDevice device;
        }

        public class Result
        {
            public NativeSlice<uint> hostTexture;
            public NativeSlice<int> pixelCounts;
            public Dictionary<Object, double> areas = new(ObjectReferenceEqualityComparer<Object>.Default);
            public double areaPerPixel;
            public RenderTexture renderTexture;
        }

        private record RenderJob : IDisposable
        {
            public bool renderPending;
            public ProcessingDevice device = ProcessingDevice.CPU;
            public JobHandle handle;
            public Action callback;
            public NativeArray<uint> tex;
            public NativeArray<int> pixels;
            public AsyncGPUReadbackRequest gpuReadbackRequest;
            public readonly Result Result = new();
            public bool active = true;

            public ComputeShader computeShader;
            public ComputeBuffer outputBuffer;

            ~RenderJob() => Dispose(false);

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public void UpdateComputeShader(ComputeShader shader, int count, Kernel main)
            {
                bool rebind = false;
                if (computeShader == null || computeShader.name != shader.name)
                {
                    computeShader = Instantiate(shader);
                    rebind = true;
                }

                if (outputBuffer == null || outputBuffer.count < count)
                {
                    ReleaseComputeBuffers();
                    count = math.max(count * 2, 1024);
                    outputBuffer = new ComputeBuffer(count, sizeof(int));
                    rebind = true;
                }

                if (!rebind)
                    return;

                if (!main.IsValid)
                    return;
                computeShader.SetBuffer(main.index, ShaderPropertyIds.OutputBuffer, outputBuffer);
            }

            private void Dispose(bool disposing)
            {
                active = false;
                if (!disposing && (tex.IsCreated || pixels.IsCreated))
                    FARLogger.Warning("Garbage collecting native arrays, use Request.Dispose() or Request.ReleaseNative() to manually release the native buffers.");
                else
                    ReleaseNative();

                Result.hostTexture = default;
                Result.pixelCounts = default;

                if (outputBuffer == null)
                    return;
                if (!disposing)
                    FARLogger.Warning("Garbage collecting ComputeBuffer. Use Request.Dispose() or Request.ReleaseComputeBuffers() to manually release it.");
                ReleaseComputeBuffers();
            }

            public void ReleaseNative()
            {
                if (tex.IsCreated)
                    tex.Dispose();
                if (pixels.IsCreated)
                    pixels.Dispose();
            }

            public void ReleaseComputeBuffers()
            {
                if (outputBuffer == null)
                    return;
                outputBuffer.Release();
                outputBuffer = null;
            }
        }

        private readonly Stack<RenderJob> requestPool = new();
        private RenderJob CurrentRenderRenderJob { get; set; }
        private readonly HashSet<RenderJob> activeJobs = new(ObjectReferenceEqualityComparer<RenderJob>.Default);

        public int ActiveJobs
        {
            get { return activeJobs.Count;  }
        }

        public bool RenderPending
        {
            get { return CurrentRenderRenderJob is { renderPending: true }; }
        }

        public void CancelPendingJobs()
        {
            if (CurrentRenderRenderJob != null) CurrentRenderRenderJob.active = false;
            foreach (RenderJob job in activeJobs)
                job.active = false;
        }

        public static T Create<T>(
            Shader shader = null,
            ComputeShader pixelCount = null,
            Transform parent = null,
            Kernel? main = null
        ) where T : ExposedSurfaceEvaluator
        {
            // one per GO so that only 1 camera is ever attached with this behaviour
            var go = new GameObject($"ExposedArea ({typeof(T).Name})");
            if (parent != null)
                go.transform.SetParent(parent);
            T self = go.AddComponent<T>();
            self.Initialize(shader, pixelCount, main);
            return self;
        }

        protected virtual void Initialize(Shader shader = null, ComputeShader pixelCount = null, Kernel? main = null)
        {
            camera = gameObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.cullingMask = 1;
            camera.orthographicSize = 3f;
            camera.nearClipPlane = 0f;
            camera.farClipPlane = 50f;
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.clear;
            camera.allowMSAA = false;
            camera.allowHDR = false;
            camera.depthTextureMode = DepthTextureMode.Depth;

            if (shader == null)
                shader = FARAssets.Instance.Shaders.ExposedSurface;
            if (pixelCount == null)
            {
                pixelCount = FARAssets.Instance.ComputeShaders.CountPixels;
                main ??= FARAssets.Instance.ComputeShaders.CountPixels.Kernel;
            }

            Shader = shader;
            SetComputeShader(pixelCount, main);
        }

        private RenderTexture GetRenderTexture()
        {
            var renderTexture = RenderTexture.GetTemporary(renderSize.x,
                                                           renderSize.y,
                                                           depthBits,
                                                           RenderTextureFormat.Default,
                                                           RenderTextureReadWrite.Default);
            renderTexture.antiAliasing = 1;
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.autoGenerateMips = false;
            camera.targetTexture = renderTexture;
            camera.depthTextureMode = DepthTextureMode.Depth;

            return renderTexture;
        }

        protected void Render(Request request)
        {
            if (RenderPending)
                throw new MethodAccessException("Rendering is still ongoing!");
            if (Tagger is null)
                throw new NullReferenceException("Please set ExposedSurfaceEvaluator.Tagger before rendering");

            Profiler.BeginSample("ExposedAreaEvaluator.RenderRequest");
            CurrentRenderRenderJob = requestPool.Count == 0 ? new RenderJob() : requestPool.Pop();
            CurrentRenderRenderJob.Result.renderTexture = GetRenderTexture();

            FitCameraToVessel(request.bounds, request.forward);
            double2 pixelSize = new double2(camera.orthographicSize * 2) / new double2(renderSize.x, renderSize.y);
            CurrentRenderRenderJob.Result.areaPerPixel = pixelSize.x * pixelSize.y;
            CurrentRenderRenderJob.renderPending = true;
            CurrentRenderRenderJob.callback = request.callback;
            CurrentRenderRenderJob.device = request.device;
            CurrentRenderRenderJob.active = true;
            if (request.device is ProcessingDevice.GPU && !SystemInfo.supportsComputeShaders)
            {
                if (!computeWarningIssued)
                {
                    FARLogger.Warning("Compute shaders are not supported on your system!");
                    computeWarningIssued = true;
                }

                CurrentRenderRenderJob.device = ProcessingDevice.CPU;
            }

            camera.Render();
            Profiler.EndSample();
        }

        private void FitCameraToVessel(Bounds bounds, Vector3 lookDir)
        {
            // size the camera viewport to fit the vessel
            float3 vesselSizes = bounds.max - bounds.min;
            float vesselSize = math.max(math.cmax(vesselSizes), 0.1f); // make sure it is never zero
            float cameraSize = 0.55f * vesselSize; // slightly more than half-size to fit the vessel in
            camera.farClipPlane = 50f + vesselSize;
            camera.orthographicSize = cameraSize;

            // place the camera so to fit vessel in view
            // https://forum.unity.com/threads/fit-object-exactly-into-perspective-cameras-field-of-view-focus-the-object.496472/#post-3229700
            const float CameraDistance = 2.0f; // Constant factor
            float cameraView =
                2.0f * math.tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView); // Visible height 1 meter in front
            float distance = CameraDistance * vesselSize / cameraView; // Combined wanted distance from the object
            distance += 0.5f * vesselSize; // Estimated offset from the center to the outside of the object

            Transform cameraTransform = camera.transform;
            cameraTransform.forward = lookDir;
            cameraTransform.position = bounds.center - distance * lookDir.normalized;
        }

        private void OnPostRender()
        {
            if (!RenderPending || !gameObject.activeSelf || !CurrentRenderRenderJob.active)
                return;

            Profiler.BeginSample("ExposedAreaEvaluator.ReadbackSetup");
            CurrentRenderRenderJob.renderPending = false;
            RenderTexture renderTexture = CurrentRenderRenderJob.Result.renderTexture;
            // only persistent works here as the readback may take too long resulting in the array getting deallocated
            int pixelCount = renderTexture.width * renderTexture.height;
            CurrentRenderRenderJob.tex = new NativeArray<uint>(pixelCount, Allocator.Persistent);
            // ref is not really needed here as Unity takes saves the pointer and size

            // TODO: try readback through a command buffer as that is not bugged
            // https://forum.unity.com/threads/asyncgpureadback-requestintonativearray-causes-invalidoperationexception-on-nativearray.1011955/
            // https://forum.unity.com/threads/asyncgpureadback-requestintonativearray-causes-invalidoperationexception-on-nativearray.1011955/#post-7347623
            AsyncGPUReadbackRequest readbackRequest;
            if (CurrentRenderRenderJob.device is ProcessingDevice.GPU)
            {
                int count = Tagger.Count;

                // https://en.wikibooks.org/wiki/Cg_Programming/Unity/Computing_Color_Histograms
                CurrentRenderRenderJob.UpdateComputeShader(PixelCountShader, count, MainKernel);

                ComputeShader shader = CurrentRenderRenderJob.computeShader;
                ComputeBuffer outputBuffer = CurrentRenderRenderJob.outputBuffer;

                CurrentRenderRenderJob.pixels = new NativeArray<int>(count, Allocator.Persistent);
                outputBuffer.SetData(CurrentRenderRenderJob.pixels);
                shader.SetTexture(MainKernel.index,
                                  ShaderPropertyIds.InputTexture,
                                  renderTexture,
                                  0,
                                  RenderTextureSubElement.Color);
                shader.SetInt(ShaderPropertyIds.Tag, (int)Tagger.Tag);

                shader.Dispatch(MainKernel.index,
                                (renderTexture.width + (int)MainKernel.threadGroupSizes.x - 1) /
                                (int)MainKernel.threadGroupSizes.x,
                                (renderTexture.height + (int)MainKernel.threadGroupSizes.y - 1) /
                                (int)MainKernel.threadGroupSizes.y,
                                1);
                readbackRequest =
                    AsyncGPUReadback.RequestIntoNativeArray(ref CurrentRenderRenderJob.pixels,
                                                            outputBuffer,
                                                            sizeof(int) * count,
                                                            0);
            }
            else
            {
                readbackRequest =
                    AsyncGPUReadback.RequestIntoNativeArray(ref CurrentRenderRenderJob.tex, renderTexture);
            }

            CurrentRenderRenderJob.gpuReadbackRequest = readbackRequest;

            StartCoroutine(CompleteRequestAsync(CurrentRenderRenderJob));
            activeJobs.Add(CurrentRenderRenderJob);
            Profiler.EndSample();
        }

        private void ReleaseRequest(RenderJob renderJob)
        {
            renderJob.ReleaseNative();
            RenderTexture.ReleaseTemporary(renderJob.Result.renderTexture);
            requestPool.Push(renderJob);
        }

        private IEnumerator CompleteRequestAsync(RenderJob renderJob)
        {
            try
            {
                while (!renderJob.gpuReadbackRequest.done)
                    yield return null;

                yield return OnGPUReadback(renderJob);
            }
            finally
            {
                activeJobs.Remove(renderJob);
                ReleaseRequest(renderJob);
            }
        }

        private IEnumerator OnGPUReadback(RenderJob renderJob)
        {
            if (renderJob.gpuReadbackRequest.hasError || !gameObject.activeSelf || !renderJob.active)
            {
                yield break;
            }

            Profiler.BeginSample("ExposedAreaEvaluator.CountJobSetup");

            renderJob.gpuReadbackRequest.WaitForCompletion();
            NativeArray<uint> texture = renderJob.gpuReadbackRequest.GetData<uint>();

            renderJob.handle.Complete();
            if (renderJob.device != ProcessingDevice.GPU)
            {
                // compute has already readback pixels
                renderJob.pixels = new NativeArray<int>(Tagger.Count, Allocator.Persistent);
                renderJob.handle = new TaggedPixelCountJob
                {
                    pixels = renderJob.pixels,
                    tag = Tagger.Tag,
                    texture = texture
                }.Schedule(texture.Length, 64);
            }

            Profiler.EndSample();

            yield return WaitForTextureProcessing(renderJob);
        }

        protected virtual void CleanJobResources()
        {
        }

        protected virtual void OnDestroy()
        {
            CancelPendingJobs();
            foreach (RenderJob request in requestPool)
                request.Dispose();

            // Coroutines are stopped so clean up any job states
            foreach (RenderJob request in activeJobs)
            {
                request.gpuReadbackRequest.WaitForCompletion();
                request.handle.Complete();
                ReleaseRequest(request);
                request.Dispose();
            }

            CleanJobResources();
        }

        protected virtual void CompleteTextureProcessing(Result result)
        {
        }

        private Result Gather(RenderJob job)
        {
            job.Result.hostTexture = job.tex;
            job.Result.pixelCounts = job.pixels;
            job.Result.areas.Clear();
            foreach (KeyValuePair<Object, int> objectIndex in Tagger)
            {
                int index = objectIndex.Value;
                job.Result.areas[objectIndex.Key] = job.pixels[index] * job.Result.areaPerPixel;
            }

            return job.Result;
        }

        private IEnumerator WaitForTextureProcessing(RenderJob renderJob)
        {
            while (!renderJob.handle.IsCompleted && renderJob.active)
                yield return null;

            if (!renderJob.active) yield break;
            renderJob.handle.Complete();
            CompleteTextureProcessing(Gather(renderJob));
            renderJob.callback?.Invoke();
        }

        [BurstCompile]
        public struct TaggedPixelCountJob : IJobParallelFor
        {
            [ReadOnly] public NativeSlice<uint> texture;
            public uint tag;
            public NativeSlice<int> pixels;

            public void Execute(int index)
            {
                uint color = texture[index];

                if (ObjectTagger.GetTag(color) != tag)
                    return;

                int partIndex = ObjectTagger.GetIndex(color);
                unsafe
                {
                    // [] returns a copy so cannot use ref with it, instead access from the buffer pointer directly
                    int* pixelBegin = (int*)pixels.GetUnsafePtr();
                    Interlocked.Increment(ref *(pixelBegin + partIndex));
                }
            }
        }
    }
}
