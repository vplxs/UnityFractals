//Based on https://github.com/noisecrime/Unity-InstancedIndirectExamples
//and https://rosettacode.org/wiki/Julia_set

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

public class JuliaIndirect : MonoBehaviour
{
    [Header("Julia Settings:")]
    public int width = 400;
    public int height = 400;
    public float unitSize = 1;
    public float zoom = 1;
    public int deltaX;
    public int deltaY;
    public int maxIterations = 255;

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


    private float cachedZoom;
    private int cachedDeltaX;
    private int cachedDeltaY;
    private const float cX = -0.7f;
    private const float cY = 0.27015f;
    private double zx;
    private double zy;
    private double temp;
    private int iter;
    // Start is called before the first frame update
    void Start()
    {
        argumentsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
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
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                zx = 1.5 * (i - width / 2) / (0.5 * zoom * width) + deltaX;
                zy = 1.0 * (j - height / 2) / (0.5 * zoom * height) + deltaY;
                iter = maxIterations;
                while (zx * zx + zy * zy < 4 && iter > 1)
                {
                    temp = zx * zx - zy * zy + cX;
                    zy = 2.0 * zx * zy + cY;
                    zx = temp;
                    iter -= 1;
                }
                positions[i * height + j] = new Vector4(i, (1 - (iter / 255f)) * 10, j, 1 - (iter / 255f));
                bounds.Encapsulate(positions[i * height + j]);
                colors[i * height + j] = new Vector4(iter / 255f, 1 - (iter / 255f), 0, 1);
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
