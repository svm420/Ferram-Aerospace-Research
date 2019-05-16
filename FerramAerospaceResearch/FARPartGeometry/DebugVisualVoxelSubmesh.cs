/*
Ferram Aerospace Research v0.15.10.1 "Lundgren"
=========================
Copyright 2019, Daumantas Kavolis, aka dkavolis

   This file is part of Ferram Aerospace Research.

   Ferram Aerospace Research is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   Ferram Aerospace Research is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with Ferram Aerospace Research.  If not, see <http: //www.gnu.org/licenses/>.

   Serious thanks:        a.g., for tons of bugfixes and code-refactorings
                stupid_chris, for the RealChuteLite implementation
                        Taverius, for correcting a ton of incorrect values
                Tetryds, for finding lots of bugs and issues and not letting me get away with them, and work on example crafts
                        sarbian, for refactoring code for working with MechJeb, and the Module Manager updates
                        ialdabaoth (who is awesome), who originally created Module Manager
                            Regex, for adding RPM support
                DaMichel, for some ferramGraph updates and some control surface-related features
                        Duxwing, for copy editing the readme

   CompatibilityChecker by Majiir, BSD 2-clause http: //opensource.org/licenses/BSD-2-Clause

   Part.cfg changes powered by sarbian & ialdabaoth's ModuleManager plugin; used with permission
    http: //forum.kerbalspaceprogram.com/threads/55219

   ModularFLightIntegrator by Sarbian, Starwaster and Ferram4, MIT: http: //opensource.org/licenses/MIT
    http: //forum.kerbalspaceprogram.com/threads/118088

   Toolbar integration powered by blizzy78's Toolbar plugin; used with permission
    http: //forum.kerbalspaceprogram.com/threads/60863
 */

using System.Collections.Generic;
using FerramAerospaceResearch.FARUtils;
using UnityEngine;
using UnityEngine.Rendering;

namespace FerramAerospaceResearch.FARPartGeometry
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class DebugVisualVoxelSubmesh : MonoBehaviour
    {
        private MeshFilter meshFilter;

        public static DebugVisualVoxelSubmesh Create(Transform parent = null, bool active = true)
        {
            GameObject go = new GameObject();
            if (parent != null)
                go.transform.SetParent(parent);
            DebugVisualVoxelSubmesh component = go.AddComponent<DebugVisualVoxelSubmesh>();
            go.SetActive(active);
            return component;
        }

        public bool Active
        {
            // ReSharper disable once UnusedMember.Global
            get => gameObject.activeSelf;
            set
            {
                gameObject.SetActive(value);
            }
        }

        public Mesh Mesh { get; private set; }

        public MeshRenderer MeshRenderer { get; private set; }

        public List<Vector3> Vertices { get; } = new List<Vector3>();

        public List<Vector2> Uvs { get; } = new List<Vector2>();

        public List<int> Triangles { get; } = new List<int>();

        protected virtual void Awake()
        {
            FARLogger.Debug("Setting up debug voxel submesh");
            Mesh = new Mesh();
            meshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();

            meshFilter.mesh = Mesh;
            MeshRenderer.material = FARAssets.ShaderCache.DebugVoxels.Material;
            MeshRenderer.material.mainTexture = FARAssets.TextureCache.VoxelTexture;
            SetupMeshRenderer();
        }

        private void SetupMeshRenderer()
        {
            MeshRenderer.lightProbeUsage = LightProbeUsage.Off;
            MeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            MeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            MeshRenderer.receiveShadows = false;
            MeshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        public void Rebuild()
        {
            Mesh.SetVertices(Vertices);
            Mesh.SetUVs(0, Uvs);
            Mesh.SetTriangles(Triangles, 0);
        }

        public void Clear()
        {
            Mesh.Clear();
            Uvs.Clear();
            Vertices.Clear();
            Triangles.Clear();
        }

        protected virtual void OnDestroy()
        {
            Clear();

            // have to clean up renderer material
            Destroy(MeshRenderer.material);
        }
    }
}
