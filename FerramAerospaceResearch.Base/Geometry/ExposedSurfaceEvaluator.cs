using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

namespace FerramAerospaceResearch.Geometry
{
    public abstract class ExposedSurfaceEvaluator : MonoBehaviour
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct ColorUintConverter
        {
            [FieldOffset(0)] public uint u;
            [FieldOffset(0)] public Color32 c;
        }

        public static Color32 UintToColor(uint value)
        {
            return new ColorUintConverter { u = value }.c;
        }

        public static uint ColorToUint(Color32 color)
        {
            return new ColorUintConverter { c = color }.u;
        }

        public Camera camera;
        private readonly Dictionary<uint, Object> registeredObjects = new();

        private NativeList<uint> sortedIds;
        private NativeHashMap<uint, int> mappedIds;

        private void UpdateIds()
        {
            if (registeredObjects.Count == sortedIds.Length)
                // still valid
                return;

            uint[] keys = registeredObjects.Keys.ToArray();
            sortedIds.CopyFrom(keys);
            sortedIds.Sort();
            for (int i = 0; i < sortedIds.Length; ++i)
                mappedIds.TryAdd(sortedIds[i], i);
        }

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

        public void SetComputeShader(ComputeShader shader, Kernel? init = default, Kernel? main = default)
        {
            PixelCountShader = shader;
            MainKernel = main ?? new Kernel(shader, "CountPixelsMain");
        }

        public enum ProcessingJobType
        {
            Mapped,
            Sorted,
            Compute,
        }

        public struct Request
        {
            public Bounds bounds;
            public Vector3 forward;
            public Action callback;
            public ProcessingJobType jobType;
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
            public ProcessingJobType jobType = ProcessingJobType.Mapped;
            public JobHandle handle;
            public Action callback;
            public NativeArray<uint> tex;
            public NativeArray<int> pixels;
            public AsyncGPUReadbackRequest gpuReadbackRequest;
            public readonly Result Result = new();

            public ComputeShader computeShader;
            public ComputeBuffer outputBuffer;
            public ComputeBuffer idBuffer;

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

                if (outputBuffer == null || idBuffer == null || outputBuffer.count < count || idBuffer.count < count)
                {
                    ReleaseComputeBuffers();
                    count = math.max(count * 2, 1024);
                    outputBuffer = new ComputeBuffer(count, sizeof(int));
                    idBuffer = new ComputeBuffer(count, sizeof(uint));
                    rebind = true;
                }

                if (!rebind)
                    return;

                if (!main.IsValid)
                    return;
                computeShader.SetBuffer(main.index, ShaderPropertyIds.OutputBuffer, outputBuffer);
                computeShader.SetBuffer(main.index, ShaderPropertyIds.SortedIds, idBuffer);
            }

            private void Dispose(bool disposing)
            {
                if (!disposing && (tex.IsCreated || pixels.IsCreated))
                    FARLogger.Warning("Garbage collecting native arrays, use Request.Dispose() or Request.ReleaseNative() to manually release the native buffers.");
                else
                    ReleaseNative();

                Result.hostTexture = default;
                Result.pixelCounts = default;

                if (outputBuffer == null && idBuffer == null)
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
                if (outputBuffer != null)
                {
                    outputBuffer.Release();
                    outputBuffer = null;
                }

                if (idBuffer == null)
                    return;
                idBuffer.Release();
                idBuffer = null;
            }
        }

        private readonly Stack<RenderJob> requestPool = new();
        private RenderJob CurrentRenderRenderJob { get; set; }
        private MaterialPropertyBlock block;

        private readonly HashSet<RenderJob> activeJobs = new(ObjectReferenceEqualityComparer<RenderJob>.Default);

        public bool RenderPending
        {
            get { return CurrentRenderRenderJob is { renderPending: true }; }
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

        protected virtual void Initialize(
            Shader shader = null,
            ComputeShader pixelCount = null,
            Kernel? init = null,
            Kernel? main = null
        )
        {
            block = new MaterialPropertyBlock();
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

            mappedIds = new NativeHashMap<uint, int>(100, Allocator.Persistent);
            sortedIds = new NativeList<uint>(100, Allocator.Persistent);

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

        protected void SetupRenderers<T>(Object obj, T renderers, MaterialPropertyBlock propertyBlock = null)
            where T : IEnumerable<Renderer>
        {
            foreach (Renderer renderer in renderers)
                SetupRenderer(obj, renderer, propertyBlock);
        }

        protected void ResetCachedRenderers()
        {
            sortedIds.Clear();
            mappedIds.Clear();
            registeredObjects.Clear();
        }

        protected void SetupRenderer(Object obj, Renderer renderer, MaterialPropertyBlock propertyBlock = null)
        {
            // unique per object, stability doesn't matter as renderers will have to be rebuilt when anything changes
            uint id = ~(uint)obj.GetInstanceID(); // alpha channels are often 0 using InstanceID only
            registeredObjects[id] = obj;

            if (propertyBlock == null)
            {
                renderer.GetPropertyBlock(block); // always overwrites block in any case
                propertyBlock = block;
            }

            Color c = UintToColor(id);
            propertyBlock.SetColor(ShaderPropertyIds.ExposedColor, c);
            FARLogger.DebugFormat("{0} ({1}): {2}", obj, id, c);
            renderer.SetPropertyBlock(propertyBlock);
        }

        protected void Render(Request request)
        {
            if (RenderPending)
                throw new MethodAccessException("Rendering is still ongoing!");

            Profiler.BeginSample("ExposedAreaEvaluator.RenderRequest");
            CurrentRenderRenderJob = requestPool.Count == 0 ? new RenderJob() : requestPool.Pop();
            CurrentRenderRenderJob.Result.renderTexture = GetRenderTexture();

            FitCameraToVessel(request.bounds, request.forward);
            double2 pixelSize = new double2(camera.orthographicSize * 2) / new double2(renderSize.x, renderSize.y);
            CurrentRenderRenderJob.Result.areaPerPixel = pixelSize.x * pixelSize.y;
            CurrentRenderRenderJob.renderPending = true;
            CurrentRenderRenderJob.callback = request.callback;
            CurrentRenderRenderJob.jobType = request.jobType;
            if (request.jobType is ProcessingJobType.Compute && !SystemInfo.supportsComputeShaders)
            {
                FARLogger.Warning("Compute shaders are not supported on your system!");
                CurrentRenderRenderJob.jobType = ProcessingJobType.Mapped;
            }

            UpdateIds();
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
            if (!RenderPending || !gameObject.activeSelf)
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
            if (CurrentRenderRenderJob.jobType is ProcessingJobType.Compute)
            {
                int count = sortedIds.Length;

                // https://en.wikibooks.org/wiki/Cg_Programming/Unity/Computing_Color_Histograms
                if (CurrentRenderRenderJob.jobType is ProcessingJobType.Compute)
                {
                    CurrentRenderRenderJob.UpdateComputeShader(PixelCountShader, count, MainKernel);
                }

                ComputeShader shader = CurrentRenderRenderJob.computeShader;
                ComputeBuffer outputBuffer = CurrentRenderRenderJob.outputBuffer;
                ComputeBuffer idBuffer = CurrentRenderRenderJob.idBuffer;

                CurrentRenderRenderJob.pixels = new NativeArray<int>(sortedIds.Length, Allocator.Persistent);
                outputBuffer.SetData(CurrentRenderRenderJob.pixels);
                idBuffer.SetData(sortedIds.AsArray());
                shader.SetTexture(MainKernel.index,
                                  ShaderPropertyIds.InputTexture,
                                  renderTexture,
                                  0,
                                  RenderTextureSubElement.Color);
                shader.SetInt(ShaderPropertyIds.IdCount, count);

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
            if (renderJob.gpuReadbackRequest.hasError || !gameObject.activeSelf)
            {
                yield break;
            }

            Profiler.BeginSample("ExposedAreaEvaluator.CountJobSetup");

            renderJob.gpuReadbackRequest.WaitForCompletion();
            NativeArray<uint> texture = renderJob.gpuReadbackRequest.GetData<uint>();

            renderJob.handle.Complete();
            if (renderJob.jobType != ProcessingJobType.Compute)
                renderJob.pixels = new NativeArray<int>(sortedIds.Length, Allocator.Persistent);
            renderJob.handle = renderJob.jobType switch
            {
                ProcessingJobType.Mapped => new MappedPixelCountJob
                {
                    ids = mappedIds,
                    pixels = renderJob.pixels,
                    texture = texture
                }.Schedule(texture.Length, 64),
                // might be faster for small numbers of objects than HashMap, needs profiling
                ProcessingJobType.Sorted => new SortedPixelCountJob
                {
                    ids = sortedIds.AsArray(),
                    pixels = renderJob.pixels,
                    texture = texture
                }.Schedule(texture.Length, 64),
                ProcessingJobType.Compute => default, // compute readback has finished directly into pixels array
                _ => throw new ArgumentOutOfRangeException()
            };

            Profiler.EndSample();

            yield return WaitForTextureProcessing(renderJob);
        }

        protected virtual void CleanJobResources()
        {
            if (mappedIds.IsCreated)
                mappedIds.Dispose();
            if (sortedIds.IsCreated)
                sortedIds.Dispose();
        }

        protected virtual void OnDestroy()
        {
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
            for (int i = 0; i < job.pixels.Length; ++i)
            {
                uint id = sortedIds[i];
                double area = job.pixels[i] * job.Result.areaPerPixel;
                if (registeredObjects.TryGetValue(id, out Object owner))
                    job.Result.areas[owner] = area;
            }

            return job.Result;
        }

        private IEnumerator WaitForTextureProcessing(RenderJob renderJob)
        {
            while (!renderJob.handle.IsCompleted)
                yield return null;

            renderJob.handle.Complete();
            CompleteTextureProcessing(Gather(renderJob));
            renderJob.callback?.Invoke();
        }

        // TODO: compare with compute shader and binary search in sorted id array
        [BurstCompile]
        public struct MappedPixelCountJob : IJobParallelFor
        {
            [ReadOnly] public NativeSlice<uint> texture;
            [ReadOnly] public NativeHashMap<uint, int> ids;
            public NativeSlice<int> pixels;

            public void Execute(int index)
            {
                uint color = texture[index];
                if (color == 0)
                    return;

                if (Hint.Unlikely(!ids.TryGetValue(color, out int partIndex)))
                    return;

                unsafe
                {
                    // [] returns a copy so cannot use ref with it, instead access from the buffer pointer directly
                    int* pixelBegin = (int*)pixels.GetUnsafePtr();
                    Interlocked.Increment(ref *(pixelBegin + partIndex));
                }
            }
        }

        private static int LowerBound(NativeSlice<uint> ids, uint id)
        {
            // binary search for id, ids must be in ascending order
            // adapted from https://en.cppreference.com/w/cpp/algorithm/lower_bound
            int count = ids.Length;
            int first = 0;

            while (count > 0)
            {
                int step = count / 2;
                int mid = first + step;
                if (ids[mid] < id)
                {
                    first = mid + 1;
                    count -= step + 1;
                }
                else
                    count = step;
            }

            return first;
        }

        [BurstCompile]
        public struct SortedPixelCountJob : IJobParallelFor
        {
            [ReadOnly] public NativeSlice<uint> texture;
            [ReadOnly] public NativeSlice<uint> ids;
            public NativeSlice<int> pixels;

            public void Execute(int index)
            {
                uint color = texture[index];
                if (color == 0)
                    return;

                int partIndex = LowerBound(ids, color);
                unsafe
                {
                    int* pixelBegin = (int*)pixels.GetUnsafePtr();
                    Interlocked.Increment(ref *(pixelBegin + partIndex));
                }
            }
        }
    }
}
