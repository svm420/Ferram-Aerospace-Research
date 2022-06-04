using Unity.Mathematics;

namespace FerramAerospaceResearch.Geometry.Exposure;

public struct CameraInfo
{
    public float3 position;
    public float3 forward;
    public float4x4 vpMatrix;
    public double projectedArea;
}
