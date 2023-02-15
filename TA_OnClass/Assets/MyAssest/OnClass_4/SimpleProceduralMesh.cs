using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))] // Require the components needed to render a mesh
public class SimpleProceduralMesh : MonoBehaviour
{
    Mesh procMesh;
    MeshFilter meshFilter;

    public Vector2 planeSize = new Vector2(1, 1);
    public int planeResolution = 2;

    [ContextMenu("Update Mesh")] // Adds function to right-click menu. Easy way to allow mesh updates in the editor.
    void UpdateMesh()
    {
        if (!procMesh)
        {
            procMesh = new Mesh {
                name = "Procedural Mesh"
            };
        }

        if (!meshFilter)
        {
            meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = procMesh;
        }

        // Prevent editor accidents, 0 would cause divide-by-zero
        planeResolution = Mathf.Max(planeResolution, 1);

        procMesh.Clear();

        //GenerateQuad();
        GenerateCube();
        //GeneratePlane(planeSize, planeResolution);
    }

    void GenerateQuad()
    {
        Vector3[] verts = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0)
        };

        Vector2[] uvs = new Vector2[] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        Vector3[] normals = new Vector3[] {
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1)
        };

        // Not necessary. Unused unless a custom shader makes use of it.
        // Use Color32 instead of Color to save a float-to-byte conversion.
        Color32[] colors = new Color32[] {
            new Color32(255, 0, 0, 255),
            new Color32(0, 255, 0, 255),
            new Color32(0, 0, 255, 255),
            new Color32(255, 255, 0, 255)
        };

        int[] tris = new int[] {
            0, 2, 1, // clockwise to determine the front face of the mesh
            1, 2, 3
        };

        procMesh.SetVertices(verts);

        // Needs to be last, to avoid array out-of-bounds errors 
        // Second param: submesh. Used for assigning materials if there are multiple of them in the Mesh Renderer
        procMesh.SetTriangles(tris, 0);

        //Vector4[] tangents = new Vector4[] {
        //    new Vector4(1, 0, 0, -1),
        //    new Vector4(1, 0, 0, -1),
        //    new Vector4(1, 0, 0, -1),
        //    new Vector4(1, 0, 0, -1)
        //};
        
        procMesh.SetNormals(normals);
        procMesh.SetUVs(0, uvs); // First param: UV channel, in [0..7] range
        procMesh.SetColors(colors);
        //procMesh.SetTangents(tangents);
    }

    void GenerateCube()
    {
        //Each vertex has three normal because it build three triangles 
        //Notice this problen when it comes to hard edge
        Vector3[] verts = {
            new Vector3 (0, 0, 0),
            new Vector3 (1, 0, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (0, 1, 0),
            new Vector3 (0, 1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (1, 0, 1),
            new Vector3 (0, 0, 1)
        };

        Vector2[] uvs = {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
        };

        int[] tris = {
	        0, 2, 1, // Front
	        0, 3, 2,
	        2, 3, 4, // Top
	        2, 4, 5,
	        1, 2, 5, // Right
	        1, 5, 6,
	        0, 7, 4, // Left
	        0, 4, 3,
	        5, 4, 7, // Back
	        5, 7, 6,
	        0, 6, 7, // Bottom
	        0, 1, 6
        };

        procMesh.SetVertices(verts);
        procMesh.SetUVs(0, uvs);
        procMesh.SetTriangles(tris, 0);

        procMesh.RecalculateNormals();
    }

    void GeneratePlane(Vector2 size, int resolution)
    {
        // Calculate the offsets for each resolution step
        float xPerStep = size.x / resolution;
        float yPerStep = size.y / resolution;

        // A 2x2 grid of quads requires a 3x3 grid of vertices
        int numVerts = (resolution + 1) * (resolution + 1);
        
        // Use arrays instead of lists. Since we know the size of all of our data in advance, we allocate the exact amount of memory we need
        // List need more memory space
        Vector3[] verts = new Vector3[numVerts];
        Vector3[] normals = new Vector3[numVerts];
        Vector2[] uvs = new Vector2[numVerts];
        Color32[] colors = new Color32[numVerts];

        for (int row = 0; row <= resolution; row++)
        {
            for (int col = 0; col <= resolution; col++)
            {
                int i = (row * resolution) + row + col;

                verts[i] = new Vector3(col * xPerStep, 0, row * yPerStep);
                
                uvs[i] = new Vector2(col, row) / resolution;
                
                normals[i] = Vector3.up;
                
                // Mimic sphere normal
                // dot product of normal and light direction
                //normals[i] = new Vector3(uvs[i].x - 0.5f, 1, 0).normalized; 
                //normals[i] = Random.insideUnitSphere;

                //colors[i] = new Color32(255, 0, 0, 255);
                //colors[i] = i % 2 == 0 ? Color.white : Color.black;
                //colors[i] = Random.ColorHSV(0, 1, 1, 1, 0.5f, 1);
            }
        }

        int triIndex = 0;
        int[] tris = new int[resolution * resolution * 2 * 3];
        for (int row = 0; row < resolution; row++)
        {
            for (int col = 0; col < resolution; col++)
            {
                int i = (row * resolution) + row + col;

                tris[triIndex + 0] = i;
                tris[triIndex + 1] = i + resolution + 1;
                tris[triIndex + 2] = i + resolution + 2;

                tris[triIndex + 3] = i;
                tris[triIndex + 4] = i + resolution + 2;
                tris[triIndex + 5] = i + 1;

                triIndex += 6;
            }
        }

        procMesh.SetVertices(verts);
        procMesh.SetNormals(normals);
        procMesh.SetUVs(0, uvs);
        procMesh.SetColors(colors);
        procMesh.SetTriangles(tris, 0);

        // Don't need to call this, SetTriangles does it for us
        //procMesh.RecalculateBounds();

        // Don't need to call this, we provide the normals already
        //procMesh.RecalculateNormals();
    }

    void Update()
    {
        // Call this if you are changing the mesh each frame
        // Also call Mesh.MarkDynamic() when setting up the mesh if you are doing this
        //UpdateMesh();
    }
}
