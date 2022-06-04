using System;
using Unity.Mathematics;

namespace FerramAerospaceResearch.Geometry.Exposure;

public record struct RenderRequest
{
    /// <summary>
    /// Camera forward direction in the same reference frame as the provided transform
    /// </summary>
    public float3 lookDir;

    /// <summary>
    /// Callback on request completion
    /// </summary>
    public Action<RenderResult, object> callback;

    /// <summary>
    /// Optional user data to pass to the callback
    /// </summary>
    public object userData;

    /// <summary>
    /// Optional debugger
    /// </summary>
    public Debugger debugger;

    /// <summary>
    /// Object to store results in, default will be used if none provided
    /// </summary>
    public RenderResult result;
}
