using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AtlasCandidateAnalyzer : EditorWindow
{
    private Dictionary<Shader, List<Renderer>> shaderGroups = new();

    [MenuItem("Tools/Analyze/Find Atlas Candidates")]
    public static void ShowWindow()
    {
        GetWindow<AtlasCandidateAnalyzer>("Atlas Candidate Finder");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Analyze Scene for Atlasing"))
        {
            AnalyzeScene();
        }

        if (shaderGroups.Count > 0)
        {
            foreach (var kvp in shaderGroups)
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField($"Shader: {kvp.Key.name} ({kvp.Value.Count} objects)", EditorStyles.boldLabel);
                foreach (var r in kvp.Value)
                {
                    EditorGUILayout.ObjectField(r.gameObject, typeof(GameObject), true);
                }
            }
        }
    }

    private void AnalyzeScene()
    {
        shaderGroups.Clear();
        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.InstanceID);

        foreach (var r in renderers)
        {
            if (r.sharedMaterial == null || r.sharedMaterial.mainTexture == null) continue;

            Shader shader = r.sharedMaterial.shader;

            if (!shaderGroups.ContainsKey(shader))
                shaderGroups[shader] = new List<Renderer>();

            shaderGroups[shader].Add(r);
        }

        Debug.Log($"✅ Found {shaderGroups.Count} unique shader groups. Objects listed in window.");
    }
}
