using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(LSystem))]
public class EditorTool : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DrawDefaultInspector();

        LSystem lSystem= (LSystem)target;
        if (GUILayout.Button("Generate the tree!"))
        {
            lSystem.Generate();
        }
    }
}
#endif
