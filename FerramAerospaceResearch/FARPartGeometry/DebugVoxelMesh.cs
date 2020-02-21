using System;
using System.Collections.Generic;
using System.Linq;
using FerramAerospaceResearch.FARUtils;
using UnityEngine;
using UnityEngine.Rendering;

namespace FerramAerospaceResearch.FARPartGeometry
{
    [RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class DebugVoxelMesh : MonoBehaviour
    {
        // limit of vertices in each mesh imposed by Unity if using 16 bit indices
        public const int MaxVerticesPerSubmesh = 65535;

        private int currentSubmesh;
        private int currentOffset;
        private int indicesPerSubmesh;
        private int verticesPerSubmesh;
        private int nextVertexCount;
        private int verticesPerVoxel;
        public Mesh Mesh { get; private set; }
        public MeshRenderer Renderer { get; private set; }
        public MeshFilter Filter { get; private set; }
        public List<Vector3> Vertices { get; } = new List<Vector3>();
        public List<List<Vector2>> Uvs { get; } = new List<List<Vector2>>();
        public List<List<int>> Indices { get; } = new List<List<int>>();

        public bool Use32BitIndices
        {
            get { return Mesh.indexFormat == IndexFormat.UInt32; }
            set { Mesh.indexFormat = value ? IndexFormat.UInt32 : IndexFormat.UInt16; }
        }

        public static DebugVoxelMesh Create(Transform parent = null)
        {
            var go = new GameObject("DebugVoxelMesh", typeof(DebugVoxelMesh));
            DebugVoxelMesh mesh = go.GetComponent<DebugVoxelMesh>();

            if (parent != null)
                go.transform.SetParent(parent);

            go.SetActive(true);
            return mesh;
        }

        private void Awake()
        {
            FARLogger.Debug("Setting up debug voxel mesh");
            Mesh = new Mesh();
            Use32BitIndices = FARSettingsScenarioModule.VoxelSettings.use32BitIndices;
            Filter = GetComponent<MeshFilter>();
            Renderer = GetComponent<MeshRenderer>();

            Filter.mesh = Mesh;

            // voxel mesh should not be affected by lights
            Renderer.lightProbeUsage = LightProbeUsage.Off;
            Renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            Renderer.shadowCastingMode = ShadowCastingMode.Off;
            Renderer.receiveShadows = false;
            Renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        public void Rebuild<T, Builder, Voxels>(Builder builder, Voxels voxels, int count)
            where Builder : IDebugVoxelMeshBuilder<T> where Voxels : IEnumerator<T>
        {
            Clear(builder, count);
            while (voxels.MoveNext())
                Add(builder, voxels.Current);
            Apply(builder);
        }

        private static void SetupNestedList<T>(List<List<T>> list, int meshes, int countPerSubmesh)
        {
            if (list.Capacity < meshes)
                list.Capacity = meshes;

            for (int i = list.Count; i < meshes; i++)
                list.Add(new List<T>());

            for (int i = 0; i < meshes; i++)
            {
                if (list[i].Capacity < countPerSubmesh)
                    list[i].Capacity = countPerSubmesh;
            }
        }

        public void Clear(bool clearMesh = true)
        {
            if (clearMesh)
                Mesh.Clear();
            Use32BitIndices = FARSettingsScenarioModule.VoxelSettings.use32BitIndices;
            Uvs.Clear();
            Vertices.Clear();
            foreach (List<int> sublist in Indices)
                sublist.Clear();

            currentSubmesh = 0;
            nextVertexCount = 0;
            currentOffset = 0;
        }

        public void Clear<Builder>(Builder builder, int count, bool clearMesh = true)
            where Builder : IDebugVoxelMeshBuilder
        {
            Clear(clearMesh);
            Reserve(builder, count);
        }

        public void Reserve<Builder>(Builder builder, int count) where Builder : IDebugVoxelMeshBuilder
        {
            int meshes;
            int voxelsPerSubmesh;
            if (Use32BitIndices)
            {
                voxelsPerSubmesh = count;
                meshes = 1;
            }
            else
            {
                voxelsPerSubmesh = MaxVerticesPerSubmesh / builder.VerticesPerVoxel;
                meshes = count / voxelsPerSubmesh + 1;
            }

            indicesPerSubmesh = voxelsPerSubmesh * builder.PrimitivesPerVoxel * IndicesPerPrimitive(builder.Topology);
            verticesPerSubmesh = voxelsPerSubmesh * builder.VerticesPerVoxel;
            nextVertexCount = verticesPerSubmesh;
            verticesPerVoxel = builder.VerticesPerVoxel;

            int nVertices = count * builder.VerticesPerVoxel;
            if (Vertices.Capacity < nVertices)
                Vertices.Capacity = nVertices;

            SetupNestedList(Indices, meshes, indicesPerSubmesh);
            SetupNestedList(Uvs, builder.UVChannels, nVertices);

            FARLogger.InfoFormat("Reserved {0} voxels in {1} submeshes", count.ToString(), meshes.ToString());
        }

        public void Add<T, Builder>(Builder builder, T voxel) where Builder : IDebugVoxelMeshBuilder<T>
        {
            builder.Build(voxel, Vertices, Uvs, Indices[currentSubmesh], currentOffset);

            // check if submesh is filled, 32 bit indices can store 4B vertices so there should be no need to check
            if (Use32BitIndices || Vertices.Count < nextVertexCount)
                return;

            currentOffset = Vertices.Count;
            currentSubmesh++;
            nextVertexCount = Vertices.Count + verticesPerSubmesh;

            // make sure the next list for indices is valid and reserve memory to for performance
            if (Indices.Count > currentSubmesh)
                return;
            Indices.Add(new List<int>());
            Indices.Capacity = indicesPerSubmesh;
        }

        public void Apply<Builder>(Builder builder) where Builder : IDebugVoxelMeshBuilder
        {
            Mesh.Clear();
            FARLogger.InfoFormat("Built voxel mesh with {0} voxels in {1} submeshes",
                                 (Vertices.Count / verticesPerVoxel).ToString(),
                                 Indices.Count.ToString());

            Mesh.SetVertices(Vertices);
            for (int i = 0; i < Uvs.Count; i++)
            {
                if (Uvs[i].Count == 0)
                    continue;
                Mesh.SetUVs(i, Uvs[i]);
            }

            //TODO: replace with Mesh.SetIndices(List<int>, ...) when using Unity 2019.3+

            if (Use32BitIndices)
            {
                // only 1 submesh
                Renderer.material = builder.MeshMaterial;
                Mesh.SetIndices(Indices[0].ToArray(), builder.Topology, 0);
            }
            else
            {
                // ignore empty index lists
                int count = Indices.Sum(list => list.Count == 0 ? 0 : 1);
                Mesh.subMeshCount = count;
                var materials = new Material[count];

                int offset = 0;
                int j = 0;
                foreach (List<int> t in Indices)
                {
                    if (t.Count == 0)
                        continue;
                    Mesh.SetIndices(t.ToArray(), builder.Topology, j, false, offset);
                    offset += verticesPerSubmesh;
                    materials[j] = builder.MeshMaterial;
                    j++;
                }

                Renderer.materials = materials;
            }
        }

        private void OnDestroy()
        {
            Mesh.Clear();
            Mesh = null;
            Filter = null;
            Renderer = null;
        }

        public static int IndicesPerPrimitive(MeshTopology topology)
        {
            return topology switch
            {
                MeshTopology.Triangles => 3,
                MeshTopology.Quads => 4,
                MeshTopology.Lines => 2,
                MeshTopology.LineStrip => 10, // variable number so use some value, list will still grow as needed
                MeshTopology.Points => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(topology), topology, null)
            };
        }
    }
}
