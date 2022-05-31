using FerramAerospaceResearch.Resources;
using UnityEngine;

namespace FerramAerospaceResearch.Geometry
{
    public struct DebugVoxel
    {
        public float Scale;
        public Vector3 Position;
        public Color Color;

        public DebugVoxel(Vector3 pos, float elementScale, Color color)
        {
            Scale = elementScale;
            Position = pos;
            Color = color;
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
            public bool HasColor
            {
                get { return true; }
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

                    if (mat == null || mat.shader == null)
                        mat = FARAssets.Instance.Shaders.DebugVoxelFallback;
                    else if (mat.HasProperty(ShaderPropertyIds._Cutoff))
                        mat.SetFloat(ShaderPropertyIds._Cutoff, FARConfig.Shaders.DebugVoxel.Cutoff);

                    mat.mainTexture = FARAssets.Instance.Textures.DebugVoxel;

                    return mat;
                }
            }

            /// <inheritdoc />
            public void Build(DebugVoxel voxel, MeshBuildData buildData, int offset)
            {
                int counter = buildData.Vertices.Count - offset;

                buildData.Vertices.Add(voxel.BottomLeft);
                buildData.Colors.Add(voxel.Color);
                buildData.Uvs[0].Add(new Vector2(0, 0));

                buildData.Vertices.Add(voxel.TopLeft);
                buildData.Colors.Add(voxel.Color);
                buildData.Uvs[0].Add(new Vector2(0, 1));

                buildData.Vertices.Add(voxel.TopRight);
                buildData.Colors.Add(voxel.Color);
                buildData.Uvs[0].Add(new Vector2(1, 1));

                buildData.Vertices.Add(voxel.BottomRight);
                buildData.Colors.Add(voxel.Color);
                buildData.Uvs[0].Add(new Vector2(1, 0));

                // left-handed triangles
                buildData.Indices.Add(counter);
                buildData.Indices.Add(counter + 1);
                buildData.Indices.Add(counter + 2);

                buildData.Indices.Add(counter);
                buildData.Indices.Add(counter + 2);
                buildData.Indices.Add(counter + 3);
            }
        }
    }
}
