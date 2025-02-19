using System;
using KSPCommunityFixes;
using UnityEngine;

/* RealChuteLite is the work of Christophe Savard (stupid_chris), and is licensed the same way than the rest of FAR is.
 * If you have any questions about this code, or want to report something, don't annoy ferram about it, ask me
 * directly on GitHub, the forums, or IRC. */

namespace FerramAerospaceResearch.RealChuteLite
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class ChuteCalculator : MonoBehaviour
    {
        private void Start()
        {
            FARLogger.Info("Initiating RealChuteLite Chute Property Calculation");
            foreach (AvailablePart part in PartLoader.Instance.loadedParts)
            {
                Part prefab = part.partPrefab;
                if (prefab == null || !(prefab.FindModuleImplementingFast<RealChuteFAR>() is RealChuteFAR module))
                    continue;
                //Updates the part's GetInfo.
                DragCubeSystem.Instance.LoadDragCubes(prefab);
                DragCube semi = prefab.DragCubes.Cubes.Find(c => c.Name == "SEMIDEPLOYED"),
                         deployed = prefab.DragCubes.Cubes.Find(c => c.Name == "DEPLOYED");
                if (semi == null || deployed == null)
                {
                    FARLogger.Info("" + part.title + " cannot find drag cube for RealChuteLite");
                    continue;
                }
                module.preDeployedDiameter = GetApparentDiameter(semi);
                module.deployedDiameter = GetApparentDiameter(deployed);
                part.moduleInfos.Find(m => m.moduleName == "RealChute").info = module.GetInfo();
            }
        }

        //Retrieves an "apparent" diameter from a DragCube
        private static float GetApparentDiameter(DragCube cube)
        {
            float area = 0;
            for (int i = 0; i < 6; i++)
                // TODO 1.2: according to API docs this method should have only 2 arguments but it has 3
                area += cube.Area[i] *
                        cube.Drag[i] *
                        PhysicsGlobals.DragCurveValue(PhysicsGlobals.SurfaceCurves,
                                                      (Vector3.Dot(Vector3.up,
                                                                   DragCubeList
                                                                       .GetFaceDirection((DragCube.DragFace)i)) +
                                                       1) *
                                                      0.5f,
                                                      0);
            return (float)Math.Max(Math.Round(Math.Sqrt(area * 0.1f * PhysicsGlobals.DragMultiplier / Math.PI) * 2,
                                              1,
                                              MidpointRounding.AwayFromZero),
                                   0.1);
        }
    }
}
