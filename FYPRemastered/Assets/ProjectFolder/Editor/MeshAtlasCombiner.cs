using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MeshAtlasCombiner : EditorWindow
{
    private List<MeshRenderer> renderers = new();
    private Material sharedMaterial;
    private Shader selectedShader;

    [MenuItem("Tools/Mesh + Texture Combiner")]
    public static void ShowWindow()
    {
        GetWindow<MeshAtlasCombiner>("Mesh + Texture Combiner");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Analyze Scene"))
        {
            AnalyzeScene();
        }

        if (renderers.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Found {renderers.Count} objects with shader: {selectedShader.name}", EditorStyles.boldLabel);

            if (GUILayout.Button("Combine and Atlas"))
            {
                CombineAndAtlas();
            }
        }
    }

    private void AnalyzeScene()
    {
        renderers.Clear();
        selectedShader = null;

        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.InstanceID))
        {
            if (r.sharedMaterial == null || r.sharedMaterial.mainTexture == null)
                continue;

            if (selectedShader == null)
                selectedShader = r.sharedMaterial.shader;

            if (r.sharedMaterial.shader == selectedShader)
            {
                renderers.Add(r);
                sharedMaterial = r.sharedMaterial;
            }
        }

        Debug.Log($"✅ Found {renderers.Count} objects using shader: {selectedShader.name}");
    }

    private void CombineAndAtlas()
    {
        if (renderers.Count == 0) return;

        List<Texture2D> textures = new();
        List<Material> materials = new();
        List<MeshFilter> filters = new();

        foreach (var r in renderers)
        {
            var tex = r.sharedMaterial.mainTexture as Texture2D;
            if (tex != null)
            {
                textures.Add(tex);
                materials.Add(r.sharedMaterial);
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null) filters.Add(mf);
            }
        }

        Texture2D atlas = new Texture2D(4096, 4096);
        Rect[] uvs = atlas.PackTextures(textures.ToArray(), 4, 4096);
        byte[] atlasBytes = atlas.EncodeToPNG();

        string atlasPath = "Assets/CombinedMeshes/Atlas.png";
        Directory.CreateDirectory("Assets/CombinedMeshes");
        File.WriteAllBytes(atlasPath, atlasBytes);
        AssetDatabase.ImportAsset(atlasPath);
        Texture2D savedAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);

        Material newMat = new Material(sharedMaterial.shader);
        newMat.mainTexture = savedAtlas;
        AssetDatabase.CreateAsset(newMat, "Assets/CombinedMeshes/CombinedMaterial.mat");

        List<CombineInstance> combine = new();
        List<Vector2[]> originalUVs = new();

        for (int i = 0; i < filters.Count; i++)
        {
            Mesh mesh = filters[i].sharedMesh;
            Vector2[] uvsOriginal = mesh.uv;
            Vector2[] uvsRemapped = new Vector2[uvsOriginal.Length];
            Rect rect = uvs[i];

            for (int j = 0; j < uvsOriginal.Length; j++)
            {
                uvsRemapped[j] = new Vector2(
                    Mathf.Lerp(rect.xMin, rect.xMax, uvsOriginal[j].x),
                    Mathf.Lerp(rect.yMin, rect.yMax, uvsOriginal[j].y));
            }

            Mesh newMesh = Object.Instantiate(mesh);
            newMesh.uv = uvsRemapped;

            CombineInstance ci = new CombineInstance
            {
                mesh = newMesh,
                transform = filters[i].transform.localToWorldMatrix
            };
            combine.Add(ci);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combine.ToArray());

        GameObject combinedGO = new GameObject("Combined_Mesh_Atlas");
        MeshFilter mfCombined = combinedGO.AddComponent<MeshFilter>();
        mfCombined.sharedMesh = combinedMesh;

        MeshRenderer mrCombined = combinedGO.AddComponent<MeshRenderer>();
        mrCombined.sharedMaterial = newMat;

        AssetDatabase.CreateAsset(combinedMesh, "Assets/CombinedMeshes/CombinedMesh.asset");

        Debug.Log("✅ Combined mesh + texture atlas created at Assets/CombinedMeshes/");
    }
}
