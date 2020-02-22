using System.Collections.Generic;
using UnityEngine;

namespace FerramAerospaceResearch.FARPartGeometry
{
    public interface IDebugVoxelMeshBuilder
    {
        /// <summary>
        /// Number of vertices per voxel used for reserving memory
        /// </summary>
        int VerticesPerVoxel { get; }

        /// <summary>
        /// Number of UV channels
        /// </summary>
        int UVChannels { get; }

        /// <summary>
        /// Type of mesh that will be built by this
        /// </summary>
        MeshTopology Topology { get; }

        /// <summary>
        /// Number of geometric primitives (line, triangle etc.) per voxel
        /// </summary>
        int PrimitivesPerVoxel { get; }

        /// <summary>
        /// Material that will be used to render the resulting mesh
        /// </summary>
        Material MeshMaterial { get; }
    }

    public interface IDebugVoxelMeshBuilder<in T> : IDebugVoxelMeshBuilder
    {
        /// <summary>
        /// Build a single voxel by appending the corresponding lists
        /// </summary>
        /// <param name="voxel">Voxel to build mesh for</param>
        /// <param name="vertices">List of mesh vertices</param>
        /// <param name="uvs">Nested list of mesh uvs, should have a list for each UV channel</param>
        /// <param name="indices">List of indices</param>
        /// <param name="offset">Offset to remove from indices before adding them to the list, for use with Unity submeshes</param>
        void Build(T voxel, List<Vector3> vertices, List<List<Vector2>> uvs, List<int> indices, int offset);
    }
}
