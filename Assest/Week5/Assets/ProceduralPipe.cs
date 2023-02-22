using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralPipe : MonoBehaviour
{
    public int segments = 6;
    public int cornerSegments = 4;
    public float cornerSharpness = 0.5f;
    public float pipeRadius = 0.5f;
    public float unitLength = 0.5f;

    MeshFilter meshFilter;
    Mesh pipeMesh;

    Vector3 currentDirection;
    Vector3 currentNormal;
    Vector3 currentPosition;
    Quaternion currentCornerRotation;

    List<Vector3> verts;
    List<int> tris;

    int direction = 0;
    int length = 0;
    bool isTurning = false;
    int nCurrentCurveSegments = 0;
    int vertCounter = 0;

    void Start()
    {
        pipeMesh = new Mesh {
            name = "Pipe Mesh"
        };

        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = pipeMesh;

        verts = new List<Vector3>();
        tris = new List<int>();

        currentPosition = Vector3.zero;
        currentDirection = Vector3.up;
        currentNormal = Vector3.right;

        length = Random.Range(5, 10);
    }

    void Update()
    {
        for (int i = 0; i < segments; i++)
        {
            float t = ((float)i / segments) * (Mathf.PI * 2);
            float r = pipeRadius;
            //float r = i % 2 == 0 ? pipeRadius : pipeRadius / 2;

            float vx = Mathf.Cos(t) * r;
            float vy = Mathf.Sin(t) * r;

            Vector3 p = currentPosition;
            p.x += vx;
            p.y += vy;

            // is the same as:

            //Vector3 p = currentPosition;
            //p += new Vector3(1, 0, 0) * vx;
            //p += new Vector3(0, 1, 0) * vy;

            // We can think of the "x" vector as being the x axis and "y" being the y axis of a plane with an origin
            // at currentPosition and normal to the currentDirection vector.
            // This is used to construct a plane that we can plot a 2D function onto.
            //Vector3 x = currentNormal.normalized;
            //Vector3 y = Vector3.Cross(currentNormal, currentDirection).normalized;
            
            //p += vx * x + vy * y;
            
            verts.Add(p);
        }

        // We need at least two rings to generate any quads
        if (verts.Count >= segments * 2)
        {
            for (int q = 0; q < segments - 1; q++, vertCounter++)
            {
                AddQuad(vertCounter, vertCounter + 1, vertCounter + segments, vertCounter + segments + 1);
            }
            AddQuad(vertCounter, vertCounter - segments + 1, vertCounter + segments, vertCounter + 1);
            vertCounter++;

            pipeMesh.SetVertices(verts.ToArray());
            pipeMesh.SetTriangles(tris.ToArray(), 0);
        }

        if (length <= 0)
        {
            direction = Random.Range(0, 5);
            isTurning = true;

            float cornerSegmentAngle = 90f / cornerSegments;

            switch (direction)
            {
                case 0: currentCornerRotation = Quaternion.Euler(+cornerSegmentAngle, 0, 0); break;
                case 1: currentCornerRotation = Quaternion.Euler(0, +cornerSegmentAngle, 0); break;
                case 2: currentCornerRotation = Quaternion.Euler(0, 0, +cornerSegmentAngle); break;
                case 3: currentCornerRotation = Quaternion.Euler(-cornerSegmentAngle, 0, 0); break;
                case 4: currentCornerRotation = Quaternion.Euler(0, -cornerSegmentAngle, 0); break;
                case 5: currentCornerRotation = Quaternion.Euler(0, 0, -cornerSegmentAngle); break;
            }

            length = cornerSegments;
        }
        else
        {
            length--;
        }

        if (isTurning)
        {
            currentDirection = currentCornerRotation * currentDirection;
            currentNormal = currentCornerRotation * currentNormal;
            currentPosition += currentDirection * cornerSharpness;

            nCurrentCurveSegments++;

            // End corner
            if (nCurrentCurveSegments >= cornerSegments)
            {
                length = Random.Range(5, 10);
                nCurrentCurveSegments = 0;
                isTurning = false;
            }
        }
        else
        {
            currentPosition += currentDirection * unitLength;
        }
    }

    void AddQuad(int v00, int v10, int v01, int v11)
    {
        tris.Add(v00);
        tris.Add(v01);
        tris.Add(v10);

        tris.Add(v10);
        tris.Add(v01);
        tris.Add(v11);
    }
}