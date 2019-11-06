//Based on https://github.com/noisecrime/Unity-InstancedIndirectExamples
//and https://www.rosettacode.org/wiki/Mandelbrot_set
/*
Copyright(c)  2019 VASILEIOS PAPALEXOPOULOS.
Permission is granted to copy, distribute and/or modify this document
under the terms of the GNU Free Documentation License, Version 1.2

or any later version published by the Free Software Foundation;

with no Invariant Sections, no Front-Cover Texts, and no Back-Cover
Texts.  A copy of the license is included in the section entitled "GNU

Free Documentation License".
*/
using UnityEngine;

public class MandelbrotInstancing : MonoBehaviour
{
    [Header("Mandelbrot Settings:")]
    public int width = 400;
    public int height = 400;
    public float unitSize = 1;
    public float zoom = 1;
    public int deltaX;
    public int deltaY;
    public float maxIterations = 255;

    [Header("Instancing Settings:")]
    public Mesh mesh;
    public Material material;

    private int count;
    private ComputeBuffer argumentsBuffer;
    private ComputeBuffer positionsBuffer;
    private ComputeBuffer colorsBuffer;
    private Vector4[] positions;
    private Vector4[] colors;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    private Bounds bounds;

    private float zmx1;
    private float zmx2;
    private float zmy1;
    private float zmy2;
    private float fn1;
    private float fn2;
    private float fn3;
    private float x;
    private float y;
    private float zr = 0;
    private float zi = 0;
    private float zr2 = 0;
    private float zi2 = 0;
    private float cr;
    private float ci;
    private float n = 1;
    private System.Random rnd;
    private float cachedZoom;
    private int cachedDeltaX;
    private int cachedDeltaY;

    // Start is called before the first frame update
    void Start()
    {
        argumentsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        rnd = new System.Random();
        fn1 = (float)rnd.NextDouble() * 20;
        fn2 = (float)rnd.NextDouble() * 20;
        fn3 = (float)rnd.NextDouble() * 20;
        CalculateSet();
        UpdateBuffers();
    }

    // Update is called once per frame
    void Update()
    {
        if (cachedZoom != zoom || cachedDeltaX != deltaX || cachedDeltaY != deltaY)
        {
            CalculateSet();
            UpdateBuffers();
        }

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argumentsBuffer);

        cachedZoom = zoom;
        cachedDeltaX = deltaX;
        cachedDeltaY = deltaY;
    }

    public void CalculateSet()
    {
        count = width * height;
        positions = new Vector4[count];
        colors = new Vector4[count];
        zmx1 = (width / 4) * zoom;
        zmx2 = 2 * (1 / zoom);
        zmy1 = (height / 4) * zoom;
        zmy2 = 2 * (1 / zoom);
        for (int i = 0; i < width; i++)
        {
            x = (i + deltaX) / zmx1 - zmx2;
            for (int j = 0; j < height; j++)
            {
                y = zmy2 - (j + deltaY) / zmy1;
                zr = 0;
                zi = 0;
                zr2 = 0;
                zi2 = 0;
                cr = x;
                ci = y;
                n = 1;
                while (n < maxIterations && (zr2 + zi2) < 4)
                {
                    zi2 = zi * zi;
                    zr2 = zr * zr;
                    zi = 2 * zi * zr + ci;
                    zr = zr2 - zi2 + cr;
                    n++;
                }
                positions[i * height + j] = new Vector4(i, 0, j, unitSize);
                bounds.Encapsulate(positions[i * height + j]);
                colors[i * height + j] = new Vector4(((n * fn1) % 255f) / 255f, ((n * fn2) % 255f) / 255f, ((n * fn3) % 255f) / 255f, 1);
            }
        }
    }

    public void UpdateBuffers()
    {
        if (positionsBuffer != null) positionsBuffer.Release();
        if (colorsBuffer != null) colorsBuffer.Release();

        positionsBuffer = new ComputeBuffer(count, 16);
        colorsBuffer = new ComputeBuffer(count, 16);

        positionsBuffer.SetData(positions);
        colorsBuffer.SetData(colors);

        material.SetBuffer("positionsBuffer", positionsBuffer);
        material.SetBuffer("colorsBuffer", colorsBuffer);

        uint numIndices = (mesh != null) ? (uint)mesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)count;
        argumentsBuffer.SetData(args);
    }

    private void OnDisable()
    {
        if (positionsBuffer != null) positionsBuffer.Release();
        positionsBuffer = null;

        if (colorsBuffer != null) colorsBuffer.Release();
        colorsBuffer = null;

        if (argumentsBuffer != null) argumentsBuffer.Release();
        argumentsBuffer = null;
    }
}
