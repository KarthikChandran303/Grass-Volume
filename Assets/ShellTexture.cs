using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class ShellTexture : MonoBehaviour
{
    
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct InputVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }
    
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct InputTriangle
    {
        public InputVertex v0;
        public InputVertex v1;
        public InputVertex v2;
    }
    
    public ComputeShader shellTextureCS;
    public Material renderingMaterial;

    [Min(1)]
    public int layers = 1;

    public float heightOffset = 0;
    
    private int kernelID;
    private int threadGroupSize;
    
    private int[] indirectArgs = new int[4] { 0, 1, 0, 0 };

    private List<InputTriangle> inputTriangles;
    
    private ComputeBuffer inputTriangleBuffer;
    private ComputeBuffer drawTriangleBuffer;
    private ComputeBuffer indirectArgsBuffer;
    
    private const int INPUT_TRIANGLE_STRIDE = (3 * (3 + 3 + 2)) * sizeof(float);
    private const int DRAW_TRIANGLE_STRIDE = (3 * (3 + 3 + 2 + 4)) * sizeof(float);
    private const int INDIRECT_ARGS_STRIDE = 4 * sizeof(int);
    
    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private int triangleCount;
    private bool initialized = false;
    private void OnEnable()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        meshRenderer = GetComponent<MeshRenderer>();
        triangleCount = mesh.triangles.Length / 3;
        SetupBuffers();
        SetupData();
        GenerateMesh();
    }
    
    private void OnDisable()
    {
        ReleaseBuffers();
    }
    
    private void OnValidate()
    {
        if (initialized)
        {
            ReleaseBuffers();
            SetupBuffers();
            SetupData();
            GenerateMesh();
        }
    }

    private void SetupBuffers()
    {
        inputTriangleBuffer = new ComputeBuffer(triangleCount, INPUT_TRIANGLE_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        drawTriangleBuffer = new ComputeBuffer(triangleCount * layers, DRAW_TRIANGLE_STRIDE, ComputeBufferType.Append);
        indirectArgsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);
    }
    
    private void ReleaseBuffers()
    {
        ReleaseBuffer(inputTriangleBuffer);
        ReleaseBuffer(drawTriangleBuffer);
        ReleaseBuffer(indirectArgsBuffer);
    }
    
    private void ReleaseBuffer(ComputeBuffer buffer)
    {
        if (buffer != null)
        {
            buffer.Release();
            buffer = null;
        }
    }

    private void SetupData()
    {
        if (mesh == null)
        {
            return;
        }
        
        inputTriangles = new List<InputTriangle>();
        for (int i = 0; i < triangleCount; i++)
        {
            InputTriangle inputTriangle = new InputTriangle();
            inputTriangles.Add(inputTriangle);
        }

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int triangle = i / 3;

            InputTriangle currentTriangle = inputTriangles[triangle];
            currentTriangle.v0.position = mesh.vertices[mesh.triangles[i]];
            currentTriangle.v0.normal = mesh.normals[mesh.triangles[i]];
            currentTriangle.v0.uv = mesh.uv[mesh.triangles[i]];
            
            currentTriangle.v1.position = mesh.vertices[mesh.triangles[i + 1]];
            currentTriangle.v1.normal = mesh.normals[mesh.triangles[i + 1]];
            currentTriangle.v1.uv = mesh.uv[mesh.triangles[i + 1]];
            
            currentTriangle.v2.position = mesh.vertices[mesh.triangles[i + 2]];
            currentTriangle.v2.normal = mesh.normals[mesh.triangles[i + 2]];
            currentTriangle.v2.uv = mesh.uv[mesh.triangles[i + 2]];
            inputTriangles[triangle] = currentTriangle;
        }
        
        inputTriangleBuffer.SetData(inputTriangles);
        drawTriangleBuffer.SetCounterValue(0);
        indirectArgsBuffer.SetData(indirectArgs);
        
        initialized = true;
    }

    private void GenerateMesh()
    {
        if(mesh == null || shellTextureCS == null || renderingMaterial == null)
        {
            return;
        }
        
        kernelID = shellTextureCS.FindKernel("ShellTexture");
        shellTextureCS.GetKernelThreadGroupSizes(kernelID, out uint threadGroupSizeX, out _, out _);
        threadGroupSize = Mathf.CeilToInt((float) triangleCount / threadGroupSizeX);

        shellTextureCS.SetBuffer(kernelID, "_InputTrianglesBuffer", inputTriangleBuffer);
        shellTextureCS.SetBuffer(kernelID, "_DrawTrianglesBuffer", drawTriangleBuffer);
        shellTextureCS.SetBuffer(kernelID, "_IndirectArgsBuffer", indirectArgsBuffer);
        shellTextureCS.SetInt("_TriangleCount", triangleCount);
        // construct matrix where the first column is the right vector, the second column is the up vector, and the third column is the forward vector
        Matrix4x4 mat = transform.localToWorldMatrix;
        // set diagonal to 1
        //mat.m11 = 1;
        shellTextureCS.SetMatrix("_LocalToWorld", mat);
        shellTextureCS.SetInt("_Layers", layers);
        shellTextureCS.SetFloat("_HeightOffset", heightOffset);
        
        renderingMaterial.SetBuffer("_DrawTrianglesBuffer", drawTriangleBuffer);
        shellTextureCS.Dispatch(kernelID, threadGroupSize, 1, 1);
        //Debug.Log("hi");
    }
    
    private void Update()
    {
        if (initialized)
        {
            Graphics.DrawProceduralIndirect(
                renderingMaterial,
                meshRenderer.bounds,
                MeshTopology.Triangles,
                indirectArgsBuffer, 
                0, null, 
                null,
                ShadowCastingMode.Off,
                true,
                gameObject.layer
                );
        }
    }
}
