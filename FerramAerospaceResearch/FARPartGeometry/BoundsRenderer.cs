using FerramAerospaceResearch.Resources;
using Unity.Mathematics;
using UnityEngine;

namespace FerramAerospaceResearch.FARPartGeometry;

public class BoundsRenderer : MonoBehaviour
{
    public Bounds VesselBounds { get; set; }

    public static BoundsRenderer Get()
    {
        GameObject camera = FlightCamera.fetch.mainCamera.gameObject;
        return camera.GetComponent<BoundsRenderer>() ?? camera.AddComponent<BoundsRenderer>();
    }

    private void Awake()
    {
        enabled = false;
    }

    private void OnPostRender()
    {
        Vessel vessel = FlightGlobals.ActiveVessel;

        GL.PushMatrix();
        GL.MultMatrix(vessel.transform.localToWorldMatrix);
        float3 min = VesselBounds.min;
        float3 max = VesselBounds.max;

        FARAssets.Instance.Shaders.LineRenderer.Material.SetPass(0);

        // front
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.green);
        GL.Vertex3(min.x, min.y, min.z);
        GL.Vertex3(min.x, max.y, min.z);
        GL.Vertex3(max.x, max.y, min.z);
        GL.Vertex3(max.x, min.y, min.z);
        GL.Vertex3(min.x, min.y, min.z);
        GL.End();

        // back
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.green);
        GL.Vertex3(min.x, min.y, max.z);
        GL.Vertex3(min.x, max.y, max.z);
        GL.Vertex3(max.x, max.y, max.z);
        GL.Vertex3(max.x, min.y, max.z);
        GL.Vertex3(min.x, min.y, max.z);
        GL.End();

        // in-between
        GL.Begin(GL.LINES);
        GL.Color(Color.green);
        GL.Vertex3(min.x, min.y, min.z);
        GL.Vertex3(min.x, min.y, max.z);

        GL.Vertex3(min.x, max.y, min.z);
        GL.Vertex3(min.x, max.y, max.z);

        GL.Vertex3(max.x, max.y, min.z);
        GL.Vertex3(max.x, max.y, max.z);

        GL.Vertex3(max.x, min.y, min.z);
        GL.Vertex3(max.x, min.y, max.z);
        GL.End();

        GL.PopMatrix();
    }
}
