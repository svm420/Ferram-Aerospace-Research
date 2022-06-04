using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using FerramAerospaceResearch.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

// TODO: reuse command buffers if objects haven't changed, manually set VP matrix before execution
// TODO: combine requests into a single command buffer and a single Graphics.ExecuteCommandBuffer call
// TODO: try using DrawMesh with MeshRenderer instead (may skip UpdateRendererBoundingVolumes if no other renderers)
// TODO: singleton executor without coroutines
// TODO: native AsyncGPUReadback plugin for OpenGL

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

    public struct TaggedInfo
    {
        public int index;
        public HashSet<Renderer> renderers;
    }

    public class ObjectTagger : IDisposable, IReadOnlyDictionary<Object, TaggedInfo>
    {
        private static readonly Stack<HashSet<Renderer>> rendererListPool = new();

        private readonly Dictionary<Object, TaggedInfo>
            objectIds = new(ObjectReferenceEqualityComparer<Object>.Default);

        private MaterialPropertyBlock block;

        public event Action<ObjectTagger> onRenderersChanged;

        public Dictionary<Object, TaggedInfo>.KeyCollection Keys
        {
            get { return objectIds.Keys; }
        }

        public Dictionary<Object, TaggedInfo>.ValueCollection Values
        {
            get { return objectIds.Values; }
        }

        public ObjectTagger()
        {
        }

        public static int GetIndex(uint value)
        {
            return (int)(value - 1);
        }

        public uint Encode(int index)
        {
            return (uint)(index + 1);
        }

        public Color SetupRenderers<T>(Object obj, T renderers, MaterialPropertyBlock propertyBlock = null)
            where T : IEnumerable<Renderer>
        {
            Color color = GetObjColor(obj);
            foreach (Renderer renderer in renderers)
            {
                SetupRenderer(renderer, color, propertyBlock);
                objectIds[obj].renderers.Add(renderer);
            }

            return color;
        }

        public void Reset()
        {
            foreach (TaggedInfo info in Values)
                rendererListPool.Push(info.renderers);
            objectIds.Clear();
            onRenderersChanged?.Invoke(this);
        }

        private Color GetObjColor(Object obj)
        {
            // unique per object, stability doesn't matter as renderers will have to be rebuilt when anything changes
            if (!objectIds.TryGetValue(obj, out TaggedInfo info))
            {
                info = new TaggedInfo
                {
                    index = objectIds.Count,
                    renderers = rendererListPool.Count == 0
                                    ? new HashSet<Renderer>(ObjectReferenceEqualityComparer<Renderer>.Default)
                                    : rendererListPool.Pop()
                };
                objectIds.Add(obj, info);
            }

            uint id = Encode(info.index);
            return ColorUintConverter.AsColor(id);
        }

        public Color SetupRenderer(Object obj, Renderer renderer, MaterialPropertyBlock propertyBlock = null)
        {
            Color color = GetObjColor(obj);
            SetupRenderer(renderer, color, propertyBlock);
            objectIds[obj].renderers.Add(renderer);
            onRenderersChanged?.Invoke(this);
            return color;
        }

        private void SetupRenderer(Renderer renderer, Color color, MaterialPropertyBlock propertyBlock = null)
        {
            if (propertyBlock == null)
            {
                block ??= new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block); // always overwrites block in any case
                propertyBlock = block;
            }

            propertyBlock.SetColor(ShaderPropertyIds._ExposedColor, color);
            renderer.SetPropertyBlock(propertyBlock);
        }

        public Dictionary<Object, TaggedInfo>.Enumerator GetEnumerator()
        {
            return objectIds.GetEnumerator();
        }

        IEnumerator<KeyValuePair<Object, TaggedInfo>> IEnumerable<KeyValuePair<Object, TaggedInfo>>.GetEnumerator()
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

        public bool TryGetValue(Object key, out TaggedInfo value)
        {
            return objectIds.TryGetValue(key, out value);
        }

        public TaggedInfo this[Object key]
        {
            get { return objectIds[key]; }
        }

        IEnumerable<Object> IReadOnlyDictionary<Object, TaggedInfo>.Keys
        {
            get { return objectIds.Keys; }
        }

        IEnumerable<TaggedInfo> IReadOnlyDictionary<Object, TaggedInfo>.Values
        {
            get { return objectIds.Values; }
        }

        public void Dispose()
        {
            Reset();
        }
    }

    public class ExposedSurfaceEvaluator : MonoBehaviour
    {
        private static bool supportsComputeShaders;
        private static bool computeWarningIssued;
        private static float3 cameraScale;
        public ObjectTagger tagger;

        public int2 renderSize = new()
        {
            x = 512,
            y = 512
        };

        private Shader replacementShader;
        private Material replacementMaterial;

        public Shader Shader
        {
            get { return replacementShader; }
            set
            {
                Debug.Assert(value != null, nameof(value) + " != null");
                replacementShader = value;
                if (replacementMaterial is null)
                    replacementMaterial = new Material(value);
                else
                    replacementMaterial.shader = value;
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
            public float4x4? toWorldMatrix;
        }

        public class Result
        {
            public NativeSlice<uint> hostTexture;
            public NativeSlice<int> pixelCounts;
            public Dictionary<Object, double> areas = new(ObjectReferenceEqualityComparer<Object>.Default);
            public double areaPerPixel;
            public RenderTexture renderTexture;
        }

        private struct Projection
        {
            public float3 position;
            public float3 forward;
            public float4x4 viewMatrix;
            public float4x4 projectionMatrix;
            public double halfSize;
        }

        [SuppressMessage("ReSharper", "Unity.NoNullPropagation")]
        protected record RenderJob : IDisposable
        {
            public float3 position;
            public float3 forward;
            public CommandBuffer commandBuffer;
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
            private int originalShader;

            // double buffering for debug visualizations
            private RenderTexture currentTexture;
            private RenderTexture secondTexture;

            ~RenderJob() => Dispose(false);

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public void ResetComputeForJob(ComputeShader shader, int count, Kernel main)
            {
                bool rebind = false;
                int shaderId = shader.GetInstanceID();
                if (shaderId != originalShader)
                {
                    originalShader = shaderId;
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

                if (!rebind || !main.IsValid)
                    return;

                computeShader.SetBuffer(main.index, ShaderPropertyIds.OutputBuffer, outputBuffer);
            }

            public void ResetNativeForJob(int2 texSize, int objectCount)
            {
                if (currentTexture is null || currentTexture.width != texSize.x || currentTexture.height != texSize.y)
                {
                    currentTexture?.Release();
                    secondTexture?.Release();
                    currentTexture = Result.renderTexture = new RenderTexture(texSize.x,
                                                                              texSize.y,
                                                                              24,
                                                                              RenderTextureFormat.Default,
                                                                              RenderTextureReadWrite.Default);
                    currentTexture.antiAliasing = 1;
                    currentTexture.filterMode = FilterMode.Point;
                    currentTexture.autoGenerateMips = false;
                    secondTexture = new RenderTexture(currentTexture);

                    if (tex.IsCreated)
                        tex.Dispose();
                    tex = new NativeArray<uint>(texSize.x * texSize.y, Allocator.Persistent);

                    // swap render textures
                    (currentTexture, secondTexture) = (secondTexture, currentTexture);
                    Result.renderTexture = currentTexture;
                }

                switch (pixels.IsCreated)
                {
                    case true when pixels.Length == objectCount:
                        unsafe
                        {
                            UnsafeUtility.MemClear(pixels.GetUnsafePtr(), pixels.Length * sizeof(int));
                        }

                        return;
                    case true:
                        pixels.Dispose();
                        break;
                }

                pixels = new NativeArray<int>(objectCount, Allocator.Persistent);
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
                currentTexture?.Release();
                secondTexture?.Release();
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
        private readonly HashSet<RenderJob> activeJobs = new(ObjectReferenceEqualityComparer<RenderJob>.Default);

        public int ActiveJobs
        {
            get { return activeJobs.Count; }
        }

        public void CancelPendingJobs()
        {
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
            T self = go.AddComponent<T>();
            if (parent != null)
                self.transform.SetParent(parent, false);
            self.Initialize(shader, pixelCount, main);
            return self;
        }

        protected virtual void Initialize(Shader shader = null, ComputeShader pixelCount = null, Kernel? main = null)
        {
            supportsComputeShaders = SystemInfo.supportsComputeShaders;
            cameraScale = new float3(1, 1, SystemInfo.usesReversedZBuffer ? -1 : 1);

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

        public void Render(Request request)
        {
            if (tagger is null)
                throw new NullReferenceException("Please set ExposedSurfaceEvaluator.tagger before rendering");
            if (tagger.Count == 0)
                return;

            Profiler.BeginSample("ExposedAreaEvaluator.RenderRequest");
            RenderJob job = requestPool.Count == 0 ? new RenderJob() : requestPool.Pop();
            job.ResetNativeForJob(renderSize, tagger.Count);

            Projection projection = FitCameraToVessel(request.bounds,
                                                      request.forward,
                                                      request.toWorldMatrix,
                                                      (float)job.Result.renderTexture.width /
                                                      job.Result.renderTexture.height);
            double pixelSize = projection.halfSize * 2 / renderSize.y;
            job.position = projection.position;
            job.forward = projection.forward;
            job.Result.areaPerPixel = pixelSize * pixelSize;
            job.callback = request.callback;
            job.device = request.device;
            job.active = true;
            if (request.device is ProcessingDevice.GPU && !supportsComputeShaders)
            {
                if (!computeWarningIssued)
                {
                    FARLogger.Warning("Compute shaders are not supported on your system!");
                    computeWarningIssued = true;
                }

                job.device = ProcessingDevice.CPU;
            }

            Profiler.BeginSample("ExposedSurfaceEvaluator.SetupCommandBuffer");
            CommandBuffer commandBuffer = job.commandBuffer ??= new CommandBuffer();
            commandBuffer.name = "ExposedSurface";
            commandBuffer.Clear();

            commandBuffer.BeginSample("ExposedSurfaceEvaluator.Render");
            // setup camera matrices since we're not using camera to render
            // commandBuffer.SetViewProjectionMatrices(proj: projection.projectionMatrix, view: projection.viewMatrix);
            // render the selected objects
            commandBuffer.SetRenderTarget(new RenderTargetIdentifier(job.Result.renderTexture),
                                          RenderBufferLoadAction.DontCare,
                                          // TODO: add request option to store/discard the texture with debug shader
                                          RenderBufferStoreAction.Store,
                                          RenderBufferLoadAction.DontCare,
                                          RenderBufferStoreAction.DontCare);
            commandBuffer.ClearRenderTarget(true, true, Color.clear);
            float4x4 vp = math.mul(GL.GetGPUProjectionMatrix(projection.projectionMatrix, true), projection.viewMatrix);
            // replacementMaterial.SetMatrix(ShaderPropertyIds._VPMatrix, vp);
            foreach (TaggedInfo info in tagger.Values)
                foreach (Renderer renderer in info.renderers)
                    commandBuffer.DrawRenderer(renderer, replacementMaterial);

            replacementMaterial.SetMatrix(ShaderPropertyIds._VPMatrix, vp);
            commandBuffer.EndSample("ExposedSurfaceEvaluator.Render");

            // dispatch compute in the same buffer
            if (job.device is ProcessingDevice.GPU)
            {
                int count = tagger.Count;

                // https://en.wikibooks.org/wiki/Cg_Programming/Unity/Computing_Color_Histograms
                job.ResetComputeForJob(PixelCountShader, count, MainKernel);

                ComputeShader shader = job.computeShader;
                ComputeBuffer outputBuffer = job.outputBuffer;

                outputBuffer.SetData(job.pixels);
                commandBuffer.SetComputeTextureParam(shader,
                                                     MainKernel.index,
                                                     ShaderPropertyIds.InputTexture,
                                                     job.Result.renderTexture,
                                                     0,
                                                     RenderTextureSubElement.Color);

                commandBuffer.BeginSample("ExposedSurfaceEvaluator.Compute");
                commandBuffer.DispatchCompute(shader,
                                              MainKernel.index,
                                              (job.Result.renderTexture.width +
                                               (int)MainKernel.threadGroupSizes.x -
                                               1) /
                                              (int)MainKernel.threadGroupSizes.x,
                                              (job.Result.renderTexture.height +
                                               (int)MainKernel.threadGroupSizes.y -
                                               1) /
                                              (int)MainKernel.threadGroupSizes.y,
                                              1);
                commandBuffer.EndSample("ExposedSurfaceEvaluator.Compute");
            }

            Profiler.EndSample();

            // do we need a separate command buffer for the compute job to run on async queue?
            Profiler.BeginSample("ExposedSurfaceEvaluator.ExecuteCommandBuffer");
            Graphics.ExecuteCommandBuffer(commandBuffer);
            Profiler.EndSample();

            Profiler.EndSample();

            FinalizeJob(job);
            OnJobSubmitted(job);
        }

        protected virtual void OnJobSubmitted(RenderJob job)
        {
        }

        private Projection FitCameraToVessel(Bounds bounds, float3 lookDir, float4x4? toWorldMatrix, float aspectRatio)
        {
            // size the camera viewport to fit the vessel
            float3 extents = bounds.extents;
            if (toWorldMatrix is not null)
            {
                var scales = new float3(math.length(toWorldMatrix.Value.c0.xyz),
                                        math.length(toWorldMatrix.Value.c1.xyz),
                                        math.length(toWorldMatrix.Value.c2.xyz));
                extents *= scales;
            }

            float maxExtent = math.max(math.cmax(extents), 0.1f); // make sure it is never zero
            float cameraSize = 1.1f * maxExtent;                  // slightly more than half-size to fit the vessel in

            // place the camera so to fit vessel in view
            // https://forum.unity.com/threads/fit-object-exactly-into-perspective-cameras-field-of-view-focus-the-object.496472/#post-3229700
            float3 center = bounds.center;
            float3 position = center - 3 * cameraSize * math.normalize(lookDir);

            if (toWorldMatrix is not null)
            {
                position = math.transform(toWorldMatrix.Value, position);
                lookDir = math.rotate(toWorldMatrix.Value, lookDir);
            }

            // http://answers.unity.com/comments/1843226/view.html
            var trs = float4x4.TRS(position, quaternion.LookRotation(lookDir, Vector3.up), cameraScale);
            float4x4 viewMatrix = math.inverse(trs);
            var projectionMatrix = float4x4.Ortho(height: cameraSize * 2,
                                                  width: cameraSize * 2 * aspectRatio,
                                                  near: 0.01f,
                                                  far: 6 * cameraSize);

            return new Projection
            {
                position = position,
                forward = lookDir,
                projectionMatrix = projectionMatrix,
                viewMatrix = viewMatrix,
                halfSize = cameraSize,
            };
        }

        private void FinalizeJob(RenderJob job)
        {
            if (!gameObject.activeSelf || !job.active)
                return;

            Profiler.BeginSample("ExposedAreaEvaluator.ReadbackSetup");
            RenderTexture renderTexture = job.Result.renderTexture;

            // TODO: try readback through a command buffer as that is not bugged
            // https://forum.unity.com/threads/asyncgpureadback-requestintonativearray-causes-invalidoperationexception-on-nativearray.1011955/
            // https://forum.unity.com/threads/asyncgpureadback-requestintonativearray-causes-invalidoperationexception-on-nativearray.1011955/#post-7347623
            AsyncGPUReadbackRequest readbackRequest;
            if (job.device is ProcessingDevice.GPU)
            {
                readbackRequest =
                    AsyncGPUReadback.RequestIntoNativeArray(ref job.pixels,
                                                            job.outputBuffer,
                                                            sizeof(int) * job.pixels.Length,
                                                            0);
            }
            else
            {
                // ref is not really needed here as Unity takes saves the pointer and size
                readbackRequest = AsyncGPUReadback.RequestIntoNativeArray(ref job.tex, renderTexture);
            }

            job.gpuReadbackRequest = readbackRequest;

            StartCoroutine(CompleteRequestAsync(job));
            activeJobs.Add(job);
            Profiler.EndSample();
        }

        private void ReleaseRequest(RenderJob renderJob)
        {
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
                renderJob.handle = new PixelCountJob
                {
                    pixels = renderJob.pixels,
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
            Destroy(replacementMaterial);
        }

        protected virtual void CompleteTextureProcessing(Result result)
        {
        }

        private Result Gather(RenderJob job)
        {
            job.Result.hostTexture = job.tex;
            job.Result.pixelCounts = job.pixels;
            job.Result.areas.Clear();
            foreach (KeyValuePair<Object, TaggedInfo> objectIndex in tagger)
            {
                int index = objectIndex.Value.index;
                job.Result.areas[objectIndex.Key] = job.pixels[index] * job.Result.areaPerPixel;
            }

            return job.Result;
        }

        private IEnumerator WaitForTextureProcessing(RenderJob renderJob)
        {
            while (!renderJob.handle.IsCompleted && renderJob.active)
                yield return null;

            if (!renderJob.active)
                yield break;
            renderJob.handle.Complete();
            CompleteTextureProcessing(Gather(renderJob));
            renderJob.callback?.Invoke();
        }

        [BurstCompile]
        public struct PixelCountJob : IJobParallelFor
        {
            [ReadOnly] public NativeSlice<uint> texture;
            public NativeSlice<int> pixels;

            public void Execute(int index)
            {
                uint color = texture[index];
                if (color == 0)
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
