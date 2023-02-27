using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UVProfile : int
{
    Fixed = 0,
    Aspect = 1,
    Uniform = 2
};

public struct Branch
{
    public Matrix4x4 transform;
    public float length;
    public float radius;
    public int numRadialSegments;
    public int numCapSegments;
    public int numHeightSegments;
    public UVProfile profile;

    public Branch(Matrix4x4 transform, float length, float radius = 0.5f, int numRadialSegments = 6, int numCapSegments = 3, int numHeightSegments = 0, UVProfile profile = UVProfile.Aspect)
    {
        this.transform = transform;
        this.length = length;
        this.radius = radius;
        this.numRadialSegments = numRadialSegments;
        this.numCapSegments = numCapSegments;
        this.numHeightSegments = numHeightSegments;
        this.profile = profile;
    }
};

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BranchMeshBuilder : MonoBehaviour
{
    private Mesh generatedMesh;
    private MeshFilter meshFilter;

    private List<CombineInstance> branchInstances = new List<CombineInstance>();

    public void Initialize(bool use32BitIndices)
    {
        if (!generatedMesh)
        {
            generatedMesh = new Mesh
            {
                name = "Generated Mesh"
            };
        }

        branchInstances.Clear();
        generatedMesh.Clear();

        generatedMesh.indexFormat = use32BitIndices ?
            UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;

        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = generatedMesh;
    }

    public void AddBranch(Branch branchData)
    {
        // Derived from https://behreajj.medium.com/making-a-capsule-mesh-via-script-in-five-3d-environments-c2214abf02db

        float radius = branchData.radius;
        float depth = branchData.length;
        int numHeightSegments = branchData.numHeightSegments;
        int longitudes = branchData.numRadialSegments;
        int latitudes = 3 + branchData.numCapSegments;
        bool calcMiddle = numHeightSegments > 0;
        int halfLats = latitudes / 2;
        int halfLatsn1 = halfLats - 1;
        int halfLatsn2 = halfLats - 2;
        int numHeightSegmentsp1 = numHeightSegments + 1;
        int lonsp1 = longitudes + 1;
        float halfDepth = depth * 0.5f;
        float summit = halfDepth + radius;

        // Vertex index offsets
        int vertOffsetNorthHemi = longitudes;
        int vertOffsetNorthEquator = vertOffsetNorthHemi + lonsp1 * halfLatsn1;
        int vertOffsetCylinder = vertOffsetNorthEquator + lonsp1;
        int vertOffsetSouthEquator = vertOffsetCylinder + (calcMiddle ? lonsp1 * numHeightSegments : 0);
        int vertOffsetSouthHemi = vertOffsetSouthEquator + lonsp1;
        int vertOffsetSouthPolar = vertOffsetSouthHemi + lonsp1 * halfLatsn2;
        int vertOffsetSouthCap = vertOffsetSouthPolar + lonsp1;

        // Initialize arrays
        int numVerts = vertOffsetSouthCap + longitudes;
        Vector3[] verts = new Vector3[numVerts];
        Vector2[] uvs = new Vector2[numVerts];
        Vector3[] normals = new Vector3[numVerts];

        float toTheta = 2f * Mathf.PI / longitudes;
        float toPhi = Mathf.PI / latitudes;
        float toTexHorizontal = 1f / longitudes;
        float toTexVertical = 1f / halfLats;

        // Calculate positions for texture coordinates vertical
        float uvAspectRatio = 1f;
        switch (branchData.profile)
        {
            case UVProfile.Aspect:
                uvAspectRatio = radius / (depth + radius + radius);
                break;
            case UVProfile.Uniform:
                uvAspectRatio = (float)halfLats / (numHeightSegmentsp1 + latitudes);
                break;
            case UVProfile.Fixed:
            default:
                uvAspectRatio = 1f / 3f;
                break;
        }

        float uvAspectNorth = 1f - uvAspectRatio;
        float uvAspectSouth = uvAspectRatio;

        Vector2[] thetaCartesian = new Vector2[longitudes];
        Vector2[] rhoThetaCartesian = new Vector2[longitudes];
        float[] sTextureCache = new float[lonsp1];

        // Polar vertices
        for (int j = 0; j < longitudes; ++j)
        {
            float jf = j;
            float sTexturePolar = 1f - ((jf + 0.5f) * toTexHorizontal);
            float theta = jf * toTheta;

            float cosTheta = Mathf.Cos(theta);
            float sinTheta = Mathf.Sin(theta);

            thetaCartesian[j] = new Vector2(cosTheta, sinTheta);
            rhoThetaCartesian[j] = new Vector2(
                radius * cosTheta,
                radius * sinTheta);

            // North
            verts[j] = new Vector3(0f, summit, 0f);
            uvs[j] = new Vector2(sTexturePolar, 1);
            normals[j] = new Vector3(0f, 1f, 0f);

            // South
            int idx = vertOffsetSouthCap + j;
            verts[idx] = new Vector3(0f, -summit, 0f);
            uvs[idx] = new Vector2(sTexturePolar, 0f);
            normals[idx] = new Vector3(0f, -1f, 0f);
        }

        // Equatorial vertices
        for (int j = 0; j < lonsp1; ++j)
        {
            float sTexture = 1f - j * toTexHorizontal;
            sTextureCache[j] = sTexture;

            // Wrap to first element upon reaching last
            int jMod = j % longitudes;
            Vector2 tc = thetaCartesian[jMod];
            Vector2 rtc = rhoThetaCartesian[jMod];

            // North equator
            int idxn = vertOffsetNorthEquator + j;
            verts[idxn] = new Vector3(rtc.x, halfDepth, -rtc.y);
            uvs[idxn] = new Vector2(sTexture, uvAspectNorth);
            normals[idxn] = new Vector3(tc.x, 0f, -tc.y);

            // South equator
            int idxs = vertOffsetSouthEquator + j;
            verts[idxs] = new Vector3(rtc.x, -halfDepth, -rtc.y);
            uvs[idxs] = new Vector2(sTexture, uvAspectSouth);
            normals[idxs] = new Vector3(tc.x, 0f, -tc.y);
        }

        // Hemisphere vertices
        for (int i = 0; i < halfLatsn1; ++i)
        {
            float ip1f = i + 1f;
            float phi = ip1f * toPhi;

            // For coordinates
            float cosPhiSouth = Mathf.Cos(phi);
            float sinPhiSouth = Mathf.Sin(phi);

            // Symmetrical hemispheres mean cosine and sine only needs to be calculated once
            float cosPhiNorth = sinPhiSouth;
            float sinPhiNorth = -cosPhiSouth;

            float rhoCosPhiNorth = radius * cosPhiNorth;
            float rhoSinPhiNorth = radius * sinPhiNorth;
            float zOffsetNorth = halfDepth - rhoSinPhiNorth;

            float rhoCosPhiSouth = radius * cosPhiSouth;
            float rhoSinPhiSouth = radius * sinPhiSouth;
            float zOffsetSouth = -halfDepth - rhoSinPhiSouth;

            // For texture coordinates
            float tTexFac = ip1f * toTexVertical;
            float cmplTexFac = 1f - tTexFac;
            float tTexNorth = cmplTexFac + uvAspectNorth * tTexFac;
            float tTexSouth = cmplTexFac * uvAspectSouth;

            int iLonsp1 = i * lonsp1;
            int vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
            int vertCurrLatSouth = vertOffsetSouthHemi + iLonsp1;

            for (int j = 0; j < lonsp1; ++j)
            {
                int jMod = j % longitudes;

                float sTexture = sTextureCache[j];
                Vector2 tc = thetaCartesian[jMod];

                // North hemisphere
                int idxn = vertCurrLatNorth + j;
                verts[idxn] = new Vector3(
                    rhoCosPhiNorth * tc.x,
                    zOffsetNorth,
                    -rhoCosPhiNorth * tc.y);
                uvs[idxn] = new Vector2(sTexture, tTexNorth);
                normals[idxn] = new Vector3(
                    cosPhiNorth * tc.x,
                    -sinPhiNorth,
                    -cosPhiNorth * tc.y);

                // South hemisphere
                int idxs = vertCurrLatSouth + j;
                verts[idxs] = new Vector3(
                    rhoCosPhiSouth * tc.x,
                    zOffsetSouth,
                    -rhoCosPhiSouth * tc.y);
                uvs[idxs] = new Vector2(sTexture, tTexSouth);
                normals[idxs] = new Vector3(
                    cosPhiSouth * tc.x,
                    -sinPhiSouth,
                    -cosPhiSouth * tc.y);
            }
        }

        // Cylinder vertices
        if (calcMiddle)
        {
            // Exclude both origin and destination edges (North and South equators) from the interpolation
            float toFac = 1 / numHeightSegmentsp1;
            int idxCylLat = vertOffsetCylinder;

            for (int h = 1; h < numHeightSegmentsp1; ++h)
            {
                float fac = h * toFac;
                float cmplFac = 1f - fac;
                float tTexture = cmplFac * uvAspectNorth + fac * uvAspectSouth;
                float z = halfDepth - depth * fac;

                for (int j = 0; j < lonsp1; ++j)
                {
                    int jMod = j % longitudes;
                    Vector2 tc = thetaCartesian[jMod];
                    Vector2 rtc = rhoThetaCartesian[jMod];
                    float sTexture = sTextureCache[j];

                    verts[idxCylLat] = new Vector3(rtc.x, z, -rtc.y);
                    uvs[idxCylLat] = new Vector2(sTexture, tTexture);
                    normals[idxCylLat] = new Vector3(tc.x, 0f, -tc.y);

                    ++idxCylLat;
                }
            }
        }

        // Triangle indices
        int lons3 = longitudes * 3; 
        int lons6 = longitudes * 6;
        int hemiLons = halfLatsn1 * lons6;

        int triOffsetNorthHemi = lons3;
        int triOffsetCylinder = triOffsetNorthHemi + hemiLons;
        int triOffsetSouthHemi = triOffsetCylinder + numHeightSegmentsp1 * lons6;
        int triOffsetSouthCap = triOffsetSouthHemi + hemiLons;

        int fsLen = triOffsetSouthCap + lons3;
        int[] tris = new int[fsLen];

        // Polar caps
        for (int i = 0, k = 0, m = triOffsetSouthCap; i < longitudes; ++i, k += 3, m += 3)
        {
            // North
            tris[k + 0] = i;
            tris[k + 1] = vertOffsetNorthHemi + i;
            tris[k + 2] = vertOffsetNorthHemi + i + 1;

            // South
            tris[m + 0] = vertOffsetSouthCap + i;
            tris[m + 1] = vertOffsetSouthPolar + i + 1;
            tris[m + 2] = vertOffsetSouthPolar + i;
        }

        // Hemispheres
        for (int i = 0, k = triOffsetNorthHemi, m = triOffsetSouthHemi; i < halfLatsn1; ++i)
        {
            int iLonsp1 = i * lonsp1;

            int vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
            int vertNextLatNorth = vertCurrLatNorth + lonsp1;

            int vertCurrLatSouth = vertOffsetSouthEquator + iLonsp1;
            int vertNextLatSouth = vertCurrLatSouth + lonsp1;

            for (int j = 0; j < longitudes; ++j, k += 6, m += 6)
            {
                // North
                int north00 = vertCurrLatNorth + j;
                int north01 = vertNextLatNorth + j;
                int north11 = vertNextLatNorth + j + 1;
                int north10 = vertCurrLatNorth + j + 1;

                tris[k + 0] = north00;
                tris[k + 1] = north11;
                tris[k + 2] = north10;

                tris[k + 3] = north00;
                tris[k + 4] = north01;
                tris[k + 5] = north11;

                // South
                int south00 = vertCurrLatSouth + j;
                int south01 = vertNextLatSouth + j;
                int south11 = vertNextLatSouth + j + 1;
                int south10 = vertCurrLatSouth + j + 1;

                tris[m + 0] = south00;
                tris[m + 1] = south11;
                tris[m + 2] = south10;

                tris[m + 3] = south00;
                tris[m + 4] = south01;
                tris[m + 5] = south11;
            }
        }

        // Cylinder
        for (int i = 0, k = triOffsetCylinder; i < numHeightSegmentsp1; ++i)
        {
            int vertCurrLat = vertOffsetNorthEquator + i * lonsp1;
            int vertNextLat = vertCurrLat + lonsp1;

            for (int j = 0; j < longitudes; ++j, k += 6)
            {
                int cy00 = vertCurrLat + j;
                int cy01 = vertNextLat + j;
                int cy11 = vertNextLat + j + 1;
                int cy10 = vertCurrLat + j + 1;

                tris[k + 0] = cy00;
                tris[k + 1] = cy11;
                tris[k + 2] = cy10;

                tris[k + 3] = cy00;
                tris[k + 4] = cy01;
                tris[k + 5] = cy11;
            }
        }

        Mesh branchMesh = new Mesh();
        branchMesh.SetVertices(verts);
        branchMesh.SetNormals(normals);
        branchMesh.SetUVs(0, uvs);
        branchMesh.SetTriangles(tris, 0);

        // Move origin to bottom
        Matrix4x4 t = branchData.transform;
        t = t * Matrix4x4.Translate(Vector3.down * branchData.length * 0.5f);

        // Store the info required to combine this branch into a single mesh
        CombineInstance combine = new CombineInstance();
        combine.mesh = branchMesh;
        combine.transform = t;
        branchInstances.Add(combine);
    }

    public void GenerateMesh()
    {
        generatedMesh.CombineMeshes(branchInstances.ToArray());
    }
}
