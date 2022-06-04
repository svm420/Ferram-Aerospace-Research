using System;
using System.IO;
using System.Threading;
using FerramAerospaceResearch.Resources;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows;
using Object = UnityEngine.Object;

namespace FerramAerospaceResearch.Geometry.Exposure;

public class Debugger : IDisposable
{
    /// <summary>
    /// Texture to store the rendered result in, will be automatically created if null or resized to match the target
    /// </summary>
    public RenderTexture texture;

    private RenderTexture tempPngTexture;
    private Texture2D pngTexture;

    public bool enabled = true;

    private Material material;
    private bool ownsMaterial;

    public Material Material
    {
        get
        {
            if (material is not null)
                return material;

            material = Object.Instantiate(FARAssets.Instance.Shaders.ExposedSurfaceDebug.Material);
            material.SetColor(ShaderPropertyIds._BackgroundColor, BackgroundColor);
            material.SetTexture(ShaderPropertyIds._ColorTex, ObjectColors);
            ownsMaterial = true;

            return material;
        }
        set
        {
            if (ReferenceEquals(material, value))
                return;

            ReleaseMaterial();
            material = value;
        }
    }

    private Color backgroundColor = Color.black;

    public Color BackgroundColor
    {
        get { return backgroundColor; }
        set
        {
            backgroundColor = value;
            Material.SetColor(ShaderPropertyIds._BackgroundColor, value);
        }
    }

    private Texture2D objectColors;

    public Texture2D ObjectColors
    {
        get { return objectColors ??= FARConfig.Voxelization.ColorMapTexture(); }
        set
        {
            objectColors = value;
            Material.SetTexture(ShaderPropertyIds._ColorTex, value);
        }
    }

    public void ReceiveResult(in RenderRequest request, RenderResult result, RenderTexture tex)
    {
        if (!enabled)
            return;

        OnReceiveResult(request, result, tex);
    }

    protected virtual void OnReceiveResult(in RenderRequest request, RenderResult result, RenderTexture tex)
    {
        RenderResources.SetupTexture(ref texture, new int2(tex.width, tex.height));
        // don't really care about the overhead of copy texture as debug should not be used in regular gameplay
        // this way command buffer does not need to be reconstructed
        Graphics.CopyTexture(tex, texture);
    }

    public void DrawTexture(float width, float height)
    {
        Rect texRect = GUILayoutUtility.GetRect(width, height);

        // Graphics.DrawTexture allows using custom materials but needs to check current event is Repaint
        if (Event.current.type != EventType.Repaint)
            return;

        Graphics.DrawTexture(texRect, texture, Material);
    }

    public void SaveToPNG([NotNull] string path, bool raw = false)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
        if (texture is null)
            throw new InvalidOperationException("Cannot save null texture as PNG");

        var size = new int2(texture.width, texture.height);
        RenderResources.SetupTexture(ref pngTexture, size);
        RenderResources.SetupTexture(ref tempPngTexture, size);

        if (raw)
        {
            RenderTexture.active = texture;
        }
        else
        {
            // cannot blit to Texture2D directly...
            Graphics.Blit(texture, tempPngTexture, Material);
            RenderTexture.active = tempPngTexture;
        }

        // ReadPixels takes cares of synchronization
        pngTexture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
        // no need for Apply since it will not be used on the gpu side anyway

        byte[] bytes = pngTexture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        FARLogger.InfoFormat("Saved rendered texture to {0}", path);
    }

    private void ReleaseMaterial()
    {
        if (!ownsMaterial || material is null)
            return;

        Object.Destroy(material);
        material = null;
        ownsMaterial = false;
    }

    private void ReleaseUnmanagedResources()
    {
        ReleaseMaterial();
        if (texture is not null)
        {
            texture.Release();
            texture = null;
        }

        if (tempPngTexture is not null)
        {
            tempPngTexture.Release();
            tempPngTexture = null;
        }

        if (pngTexture is not null)
        {
            Object.Destroy(pngTexture);
            pngTexture = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Debugger()
    {
        Dispose(false);
    }
}
