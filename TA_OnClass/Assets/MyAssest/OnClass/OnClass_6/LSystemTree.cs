using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BranchMeshBuilder))]
public class LSystemTree : MonoBehaviour
{
    [Serializable]
    public struct LSystemRule
    {
        public char source;
        public string target;
    }

    [Header("Generation")]
    public float angle = 25f;
    public float length = 1f;
    [Min(0.01f)] public float radius = 0.25f;
    [Range(1, 8)] public int iterations = 3;

    [Header("Mesh")]
    [Range(1, 32)] public int radialSegments = 6;
    [Range(1, 10)] public int capSegments = 3;
    public bool use32BitIndices = false;

    [Header("L-System")]
    public string axiom = "F";
    public List<LSystemRule> rules = new List<LSystemRule>();

    private BranchMeshBuilder branchBuilder;
    private List<Branch> branches = new List<Branch>();

    char[] GenerateSentence(char[] sentence, int iterationCount = 0)
    {
        List<char> nextSentence = new List<char>();
        for (int i = 0; i < sentence.Length; i++)
        {
            char currentChar = sentence[i];
            bool ruleApplied = false;

            foreach (LSystemRule rule in rules)
            {
                if (currentChar == rule.source)
                {
                    ruleApplied = true;
                    nextSentence.AddRange(rule.target.ToCharArray());
                    break;
                }
            }

            if (!ruleApplied)
            {
                nextSentence.Add(currentChar);
            }
        }

        sentence = nextSentence.ToArray();

        iterationCount++;
        if (iterationCount < Mathf.Min(iterations, 9))
        {
            return GenerateSentence(sentence, iterationCount);
        }
        else
        {
            return sentence;
        }
    }

    void GenerateBranches(bool printDebug = false)
    {
        branches.Clear();

        char[] sentence = GenerateSentence(axiom.ToCharArray());
        if (printDebug)
        {
            Debug.Log(new string(sentence));
        }

        List<Matrix4x4> transformStack = new List<Matrix4x4>();
        transformStack.Add(transform.localToWorldMatrix);
        Matrix4x4 workingTransform = transformStack[0];

        for (int i = 0; i < sentence.Length; i++)
        {
            char currentChar = sentence[i];

            if (currentChar == 'F') // Move forward and add branch
            {
                workingTransform *= Matrix4x4.Translate(Vector3.up * length);
                branches.Add(new Branch(workingTransform, length, radius, radialSegments, capSegments));
            }
            else if (currentChar == '+') // Rotate counter-clockwise
            {
                workingTransform *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, +angle));
            }
            else if (currentChar == '-') // Rotate clockwise
            {
                workingTransform *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, -angle));
            }
            else if (currentChar == '[') // Save transform
            {
                transformStack.Add(workingTransform);
            }
            else if (currentChar == ']') // Recall transform
            {
                int lastIndex = transformStack.Count - 1;

                workingTransform = transformStack[lastIndex];
                transformStack.RemoveAt(lastIndex);
            }
        }
    }

    [ContextMenu("Update Mesh")]
    void UpdateMesh()
    {
        if (!branchBuilder)
        {
            branchBuilder = GetComponent<BranchMeshBuilder>();
        }

        GenerateBranches(true);

        branchBuilder.Initialize(use32BitIndices);
        foreach (Branch branch in branches)
        {
            branchBuilder.AddBranch(branch);
        }
        branchBuilder.GenerateMesh();
    }

    void OnDrawGizmos()
    {
        GenerateBranches();

        Gizmos.color = Color.white;
        foreach (Branch branch in branches)
        {
            Vector3 currentPos = branch.transform.GetPosition();
            Matrix4x4 t = branch.transform * Matrix4x4.Translate(Vector3.down * branch.length);

            Gizmos.DrawLine(currentPos, t.GetPosition());
        }
    }
}
