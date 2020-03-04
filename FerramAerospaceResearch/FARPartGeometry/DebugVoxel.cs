using System.Collections.Generic;
using FerramAerospaceResearch.Resources;
using UnityEngine;

namespace FerramAerospaceResearch.FARPartGeometry
{
    public struct DebugVoxel
    {
        public float Scale;
        public Vector3 Position;

        public DebugVoxel(Vector3 pos, float elementScale)
        {
            Scale = elementScale;
            Position = pos;
        }

        public Vector3 BottomLeft
        {
            get { return new Vector3(Position.x - Scale, Position.y - Scale, Position.z); }
        }

        public Vector3 BottomRight
        {
            get { return new Vector3(Position.x + Scale, Position.y - Scale, Position.z); }
        }

        public Vector3 TopRight
        {
            get { return new Vector3(Position.x + Scale, Position.y + Scale, Position.z); }
        }

        public Vector3 TopLeft
        {
            get { return new Vector3(Position.x - Scale, Position.y + Scale, Position.z); }
        }

        public struct Builder : IDebugVoxelMeshBuilder<DebugVoxel>
        {
            /// <inheritdoc />
            public int VerticesPerVoxel
            {
                get { return 4; }
            }

            /// <inheritdoc />
            public int UVChannels
            {
                get { return 1; }
            }

            /// <inheritdoc />
            public MeshTopology Topology
            {
                // using triangles since they have better performance than quads
                get { return MeshTopology.Triangles; }
            }

            /// <inheritdoc />
            public int PrimitivesPerVoxel
            {
                get { return 2; }
            }

            /// <inheritdoc />
            public Material MeshMaterial
            {
                get
                {
                    Material mat = FARAssets.Instance.Shaders.DebugVoxel;

                    if (mat?.shader == null)
                        mat = FARAssets.Instance.Shaders.DebugVoxelFallback;
                    else if (mat.HasProperty(ShaderPropertyIds.Cutoff))
                        mat.SetFloat(ShaderPropertyIds.Cutoff, FARConfig.Shaders.DebugVoxel.Cutoff);

                    mat.mainTexture = FARAssets.Instance.Textures.DebugVoxel;

                    return mat;
                }
            }

            /// <inheritdoc />
            public void Build(
                DebugVoxel voxel,
                List<Vector3> vertices,
                List<List<Vector2>> uvs,
                List<int> indices,
                int offset
            )
            {
                int counter = vertices.Count - offset;

                vertices.Add(voxel.BottomLeft);
                uvs[0].Add(new Vector2(0, 0));

                vertices.Add(voxel.TopLeft);
                uvs[0].Add(new Vector2(0, 1));

                vertices.Add(voxel.TopRight);
                uvs[0].Add(new Vector2(1, 1));

                vertices.Add(voxel.BottomRight);
                uvs[0].Add(new Vector2(1, 0));

                // left-handed triangles
                indices.Add(counter);
                indices.Add(counter + 1);
                indices.Add(counter + 2);

                indices.Add(counter);
                indices.Add(counter + 2);
                indices.Add(counter + 3);
            }
        }
    }
}
