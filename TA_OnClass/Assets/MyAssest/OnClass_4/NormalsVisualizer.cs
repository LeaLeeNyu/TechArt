using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshFilter))]
public class NormalsVisualizer : Editor
{
    private const string EDITOR_PREF_KEY_ENABLE_DEBUG = "_enable_debug";
    private const string EDITOR_PREF_KEY_NORMALS_LENGTH = "_normals_length";
    private const string EDITOR_PREF_KEY_SHOW_VERT_NUMBERS = "_show_labels";
    private const string EDITOR_PREF_KEY_SHOW_EDGES = "_show_edges";

    private Mesh mesh;
    private MeshFilter mf;
    private Vector3[] verts;
    private Vector3[] normals;
    private int[] tris;
    private bool enableDebug = false;
    private float normalsLength = 0f;
    private bool showVertexNumbers = false;
    private bool showEdges = false;

    private void OnEnable()
    {
        mf = target as MeshFilter;
        if (mf != null)
        {
            mesh = mf.sharedMesh;
        }

        enableDebug = EditorPrefs.GetBool(EDITOR_PREF_KEY_ENABLE_DEBUG);
        normalsLength = EditorPrefs.GetFloat(EDITOR_PREF_KEY_NORMALS_LENGTH);
        showVertexNumbers = EditorPrefs.GetBool(EDITOR_PREF_KEY_SHOW_VERT_NUMBERS);
        showEdges = EditorPrefs.GetBool(EDITOR_PREF_KEY_SHOW_EDGES);
    }

    private void OnSceneGUI()
    {
        if (!enableDebug)
        {
            return;
        }

        if (mesh == null)
        {
            return;
        }

        Handles.matrix = mf.transform.localToWorldMatrix;
        
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 20;

        verts = mesh.vertices;
        normals = mesh.normals;
        tris = mesh.triangles;

        if (showEdges)
        {
            for (int i = 0; i < tris.Length; i += 3)
            {
                Handles.color = Color.red;
                Handles.DrawLine(verts[tris[i + 0]], verts[tris[i + 1]], 3);

                Handles.color = Color.green;
                Handles.DrawLine(verts[tris[i + 1]], verts[tris[i + 2]], 6);

                Handles.color = Color.blue;
                Handles.DrawLine(verts[tris[i + 2]], verts[tris[i + 0]], 9);
            }
        }

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            if (normals.Length > 0 && normalsLength > 0f)
            {
                Handles.color = Color.yellow;
                Handles.DrawLine(verts[i], verts[i] + normals[i] * normalsLength);
            }

            if (showVertexNumbers)
            {
                GUI.color = Color.black;
                Handles.Label(verts[i], i.ToString(), labelStyle);
            }
        }

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();

        enableDebug = EditorGUILayout.Toggle("Show debug info", enableDebug);
        showVertexNumbers = EditorGUILayout.Toggle("Show vertex numbers", showVertexNumbers);
        showEdges = EditorGUILayout.Toggle("Show edges", showEdges);
        normalsLength = EditorGUILayout.FloatField("Normals length", normalsLength);

        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool(EDITOR_PREF_KEY_ENABLE_DEBUG, enableDebug);
            EditorPrefs.SetFloat(EDITOR_PREF_KEY_NORMALS_LENGTH, normalsLength);
            EditorPrefs.SetBool(EDITOR_PREF_KEY_SHOW_VERT_NUMBERS, showVertexNumbers);
            EditorPrefs.SetBool(EDITOR_PREF_KEY_SHOW_EDGES, showEdges);
        }
    }
}
