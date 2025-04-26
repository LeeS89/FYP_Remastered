using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Rendering;

public class SceneTextureAtlasWithUV : EditorWindow
{
    [MenuItem("Tools/Generate Scene Texture Atlas (with UVs)")]
    public static void ShowWindow()
    {
        GetWindow<SceneTextureAtlasWithUV>("Scene Atlas Generator");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Generate Scene Atlas"))
        {
            GenerateAndApplyAtlas();
        }
    }

    static readonly string[] textureProps = {
        "_BaseMap", "_BumpMap", "_OcclusionMap", "_EmissionMap", "_MetallicGlossMap"
    };

    private static void GenerateAndApplyAtlas()
    {
        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.InstanceID);
        var meshes = new List<Mesh>();
        var combineInstances = new List<CombineInstance>();
        var textureGroups = new Dictionary<string, List<Texture2D>>();
        var uvsPerMesh = new List<Vector2[]>();
        var rectMap = new Dictionary<string, Rect>();
        var materialRefs = new List<Material>();
        var originalTransforms = new List<Transform>();

        foreach (string prop in textureProps)
            textureGroups[prop] = new List<Texture2D>();

        foreach (var r in renderers)
        {
            var mf = r.GetComponent<MeshFilter>();
            if (mf == null || r.sharedMaterial == null) continue;

            Mesh mesh = Object.Instantiate(mf.sharedMesh);
            meshes.Add(mesh);
            uvsPerMesh.Add(mesh.uv);
            materialRefs.Add(r.sharedMaterial);
            originalTransforms.Add(r.transform);

            foreach (string prop in textureProps)
            {
                Texture2D tex = r.sharedMaterial.GetTexture(prop) as Texture2D;
                textureGroups[prop].Add(tex != null ? MakeReadable(tex) : Texture2D.blackTexture);
            }
        }

        Dictionary<string, Texture2D> atlases = new();
        Dictionary<string, Rect[]> rectArrays = new();

        foreach (var kvp in textureGroups)
        {
            Texture2D atlas = new Texture2D(4096, 4096);
            Rect[] rects = atlas.PackTextures(kvp.Value.ToArray(), 2, 4096);
            string savePath = $"Assets/Atlas_{kvp.Key.Replace("_", "")}.png";
            File.WriteAllBytes(savePath, atlas.EncodeToPNG());
            AssetDatabase.ImportAsset(savePath);
            SetImportSettings(savePath, kvp.Key);
            Texture2D loaded = AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
            atlases[kvp.Key] = loaded;
            rectArrays[kvp.Key] = rects;
        }

        for (int i = 0; i < meshes.Count; i++)
        {
            Vector2[] originalUVs = uvsPerMesh[i];
            Vector2[] newUVs = new Vector2[originalUVs.Length];

            Rect baseRect = rectArrays["_BaseMap"][i]; // Assuming all props share order

            for (int j = 0; j < newUVs.Length; j++)
            {
                newUVs[j] = new Vector2(
                    Mathf.Lerp(baseRect.xMin, baseRect.xMax, originalUVs[j].x),
                    Mathf.Lerp(baseRect.yMin, baseRect.yMax, originalUVs[j].y)
                );
            }

            meshes[i].uv = newUVs;

            combineInstances.Add(new CombineInstance
            {
                mesh = meshes[i],
                transform = originalTransforms[i].localToWorldMatrix
            });
        }

        Material combinedMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        foreach (var atlas in atlases)
        {
            combinedMat.SetTexture(atlas.Key, atlas.Value);
        }

        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = IndexFormat.UInt32;
        finalMesh.CombineMeshes(combineInstances.ToArray(), true, true);

        GameObject result = new GameObject("Scene_Combined_Atlas");
        result.AddComponent<MeshFilter>().sharedMesh = finalMesh;
        result.AddComponent<MeshRenderer>().sharedMaterial = combinedMat;

        Selection.activeGameObject = result;
        Debug.Log("✅ Scene objects combined with full texture atlases and UVs.");
    }

    private static Texture2D MakeReadable(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static void SetImportSettings(string path, string typeHint)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) return;

        importer.isReadable = true;
        importer.sRGBTexture = !typeHint.Contains("Bump") && !typeHint.Contains("Metallic") && !typeHint.Contains("Occlusion");
        if (typeHint.Contains("Bump"))
            importer.textureType = TextureImporterType.NormalMap;

        importer.SaveAndReimport();
    }
}






/*using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class URPTextureAtlasGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Texture Atlas")]
    public static void ShowWindow()
    {
        GetWindow<URPTextureAtlasGenerator>("URP Texture Atlas Generator");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Generate Atlas"))
        {
            GenerateTextureAtlas();
        }
    }

    private static void EnsureReadable(Texture2D texture)
    {
        if (!texture.isReadable)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }
    }

    private static void GenerateTextureAtlas()
    {
        Dictionary<string, List<Texture2D>> textureGroups = new Dictionary<string, List<Texture2D>>
        {
            { "_BaseMap", new List<Texture2D>() },
            { "_BumpMap", new List<Texture2D>() }, // URP uses _BumpMap instead of _NormalMap
            { "_OcclusionMap", new List<Texture2D>() },
            { "_EmissionMap", new List<Texture2D>() },
            { "_MetallicGlossMap", new List<Texture2D>() } // URP uses MetallicGlossMap instead of _MetallicMap
        };

        foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.InstanceID))
        {
            if (renderer.sharedMaterial != null)
            {
                foreach (var key in textureGroups.Keys)
                {
                    if (renderer.sharedMaterial.HasProperty(key))
                    {
                        Texture2D tex = renderer.sharedMaterial.GetTexture(key) as Texture2D;
                        if (tex != null)
                        {
                            EnsureReadable(tex);
                            textureGroups[key].Add(tex);
                        }
                    }
                }
            }
        }

        foreach (var group in textureGroups)
        {
            if (group.Value.Count > 0)
            {
                Texture2D atlas = new Texture2D(1024, 1024);
                Rect[] rects = atlas.PackTextures(group.Value.ToArray(), 2, 1024);
                byte[] atlasBytes = atlas.EncodeToPNG();
                string atlasPath = $"Assets/{group.Key}_Atlas.png";
                System.IO.File.WriteAllBytes(atlasPath, atlasBytes);
                AssetDatabase.Refresh();
                ApplyAtlasToScene(atlasPath, group.Key);
                Debug.Log($"Saved {group.Key} Atlas and applied it to scene objects.");
            }
        }
    }

    private static void ApplyAtlasToScene(string atlasPath, string textureType)
    {
        Texture2D atlasTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
        if (atlasTexture != null)
        {
            foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.InstanceID))
            {
                Material originalMaterial = renderer.sharedMaterial;
                if (originalMaterial.HasProperty(textureType))
                {
                    // Preserve original material settings
                    Material newMaterial = new Material(originalMaterial);
                    newMaterial.SetTexture(textureType, atlasTexture);
                    renderer.sharedMaterial = newMaterial;
                }
            }

            Debug.Log($"Applied {textureType} Atlas correctly to scene objects!");
        }
    }

}
*/