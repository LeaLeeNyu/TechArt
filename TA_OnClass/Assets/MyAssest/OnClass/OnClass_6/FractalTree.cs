using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BranchMeshBuilder))]
public class FractalTree : MonoBehaviour
{
    [Header("Generation")]
    public float angle = 45f;
    public float twist = 120f;
    public float length = 4f;
    public float lengthDecay = 0.67f;
    [Min(0.01f)] public float radius = 0.25f;
    public float radiusDecay = 0.67f;
    [Range(1, 10)] public int iterations = 5;
    public bool use3D = false;

    [Header("Mesh")]
    [Range(1, 32)] public int radialSegments = 6;
    [Range(1, 10)] public int capSegments = 3;
    public bool use32BitIndices = false;

    private BranchMeshBuilder branchBuilder;
    private List<Branch> branches = new List<Branch>();

    void GenerateBranches()
    {
        branches.Clear();

        if (use3D)
        {
            GenerateBranch3D(transform.localToWorldMatrix, length, radius);
        }
        else
        {
            GenerateBranch2D(transform.localToWorldMatrix, length, radius);
        }
    }

    [ContextMenu("Update Mesh")]
    void UpdateMesh()
    {
        if (!branchBuilder)
        {
            branchBuilder = GetComponent<BranchMeshBuilder>();
        }

        GenerateBranches();

        branchBuilder.Initialize(use32BitIndices);

        foreach (Branch branch in branches)
        {
            branchBuilder.AddBranch(branch);
        }

        branchBuilder.GenerateMesh();
    }

    void GenerateBranch2D(Matrix4x4 t, float length, float radius, int iterationCount = 0)
    {
        t = t * Matrix4x4.Translate(Vector3.up * length);

        branches.Add(new Branch(t, length, radius, radialSegments, capSegments));

        if (iterationCount < iterations)
        {
            iterationCount++;

            Matrix4x4 t1 = t * Matrix4x4.Rotate(Quaternion.Euler(0, 0, +angle));
            Matrix4x4 t2 = t * Matrix4x4.Rotate(Quaternion.Euler(0, 0, -angle));

            float newLength = length * lengthDecay;
            float newRadius = radius * radiusDecay;

            GenerateBranch2D(t1, newLength, newRadius, iterationCount);
            GenerateBranch2D(t2, newLength, newRadius, iterationCount);
        }
    }

    void GenerateBranch3D(Matrix4x4 t, float length, float radius, int iterationCount = 0)
    {
        t = t * Matrix4x4.Translate(Vector3.up * length);

        branches.Add(new Branch(t, length, radius, radialSegments, capSegments));

        if (iterationCount < iterations)
        {
            iterationCount++;

            Matrix4x4 t1 = t * Matrix4x4.Rotate(Quaternion.Euler(0, twist * 1, angle));
            Matrix4x4 t2 = t * Matrix4x4.Rotate(Quaternion.Euler(0, twist * 2, angle));
            Matrix4x4 t3 = t * Matrix4x4.Rotate(Quaternion.Euler(0, twist * 3, angle));

            float newLength = length * lengthDecay;
            float newRadius = radius * radiusDecay;

            GenerateBranch3D(t1, newLength, newRadius, iterationCount);
            GenerateBranch3D(t2, newLength, newRadius, iterationCount);
            GenerateBranch3D(t3, newLength, newRadius, iterationCount);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        GenerateBranches();

        foreach (Branch branch in branches)
        {
            Vector3 startPos = branch.transform.GetPosition();
            Matrix4x4 t = branch.transform * Matrix4x4.Translate(Vector3.down * branch.length);

            Gizmos.DrawLine(startPos, t.GetPosition());
        }
    }
}
