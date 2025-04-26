using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MeshAndTextureCombiner : EditorWindow
{
    private List<GameObject> targetObjects = new List<GameObject>();
    private Vector2 scroll;

    [MenuItem("Tools/Combined Mesh & Texture Atlas Tool")]
    public static void ShowWindow()
    {
        GetWindow<MeshAndTextureCombiner>("Mesh & Texture Atlas");
    }

    private void OnGUI()
    {
        GUILayout.Label("Combine Meshes + Texture Atlasing", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Selected Objects"))
        {
            foreach (var obj in Selection.gameObjects)
            {
                if (!targetObjects.Contains(obj))
                    targetObjects.Add(obj);
            }
        }

        if (GUILayout.Button("Clear"))
        {
            targetObjects.Clear();
        }

        GUILayout.Space(5);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var obj in targetObjects)
        {
            EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        if (GUILayout.Button("Combine + Atlas"))
        {
            CombineAndAtlas();
        }
    }

    private void CombineAndAtlas()
    {
        List<CombineInstance> combine = new();
        List<Texture2D> baseMaps = new();
        List<Texture2D> normalMaps = new();
        List<Texture2D> metallicMaps = new();
        List<Texture2D> occlusionMaps = new();
        List<Texture2D> emissionMaps = new();

        List<Vector2[]> originalUVs = new();
        List<Mesh> meshes = new();

        foreach (var obj in targetObjects)
        {
            var mf = obj.GetComponent<MeshFilter>();
            var mr = obj.GetComponent<MeshRenderer>();
            if (mf == null || mr == null || mr.sharedMaterial == null)
                continue;

            baseMaps.Add(GetTexture(mr.sharedMaterial, "_BaseMap") ?? Texture2D.blackTexture);
            normalMaps.Add(GetTexture(mr.sharedMaterial, "_BumpMap") ?? Texture2D.blackTexture);
            metallicMaps.Add(GetTexture(mr.sharedMaterial, "_MetallicGlossMap") ?? Texture2D.blackTexture);
            occlusionMaps.Add(GetTexture(mr.sharedMaterial, "_OcclusionMap") ?? Texture2D.blackTexture);
            emissionMaps.Add(GetTexture(mr.sharedMaterial, "_EmissionMap") ?? Texture2D.blackTexture);

            Mesh meshCopy = Instantiate(mf.sharedMesh);
            meshes.Add(meshCopy);
            originalUVs.Add(meshCopy.uv);
        }

        Texture2D Atlas(string name, List<Texture2D> list)
        {
            Texture2D atlas = new(4096, 4096);
            Rect[] rects = atlas.PackTextures(list.ToArray(), 2, 4096);
            string path = $"Assets/{name}.png";
            File.WriteAllBytes(path, atlas.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            FixTextureImportSettings(path, name.ToLower());
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // Create atlases
        Texture2D baseAtlas = Atlas("Atlas_BaseMap", baseMaps);
        Texture2D normalAtlas = Atlas("Atlas_NormalMap", normalMaps);
        Texture2D metallicAtlas = Atlas("Atlas_MetallicMap", metallicMaps);
        Texture2D occlusionAtlas = Atlas("Atlas_OcclusionMap", occlusionMaps);
        Texture2D emissionAtlas = Atlas("Atlas_EmissionMap", emissionMaps);

        // Remap UVs using base atlas rects
        Rect[] rects = baseAtlas.PackTextures(baseMaps.ToArray(), 2, 4096); // again to get the same rects used in packing

        for (int i = 0; i < meshes.Count; i++)
        {
            Vector2[] uvs = originalUVs[i];
            Rect r = rects[i];
            Vector2[] newUVs = new Vector2[uvs.Length];

            for (int j = 0; j < uvs.Length; j++)
            {
                newUVs[j] = new Vector2(
                    Mathf.Lerp(r.xMin, r.xMax, uvs[j].x),
                    Mathf.Lerp(r.yMin, r.yMax, uvs[j].y)
                );
            }

            meshes[i].uv = newUVs;
            combine.Add(new CombineInstance
            {
                mesh = meshes[i],
                transform = targetObjects[i].transform.localToWorldMatrix
            });
        }

        // Final material
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetTexture("_BaseMap", baseAtlas);
        mat.SetTexture("_BumpMap", normalAtlas);
        mat.SetTexture("_MetallicGlossMap", metallicAtlas);
        mat.SetTexture("_OcclusionMap", occlusionAtlas);
        mat.SetTexture("_EmissionMap", emissionAtlas);
        mat.EnableKeyword("_NORMALMAP");
        mat.EnableKeyword("_METALLICGLOSSMAP");
        mat.EnableKeyword("_OCCLUSIONMAP");
        mat.EnableKeyword("_EMISSION");

        // Combine mesh
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combine.ToArray(), true, true);

        GameObject go = new GameObject("Combined_Mesh_Atlas");
        go.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;

        Selection.activeGameObject = go;
        Debug.Log("✅ Combined mesh and all texture atlases created.");
    }


    private Texture2D GetTexture(Material mat, string property)
    {
        if (mat.HasProperty(property))
            return mat.GetTexture(property) as Texture2D;
        return null;
    }

    private void FixTextureImportSettings(string path, string typeHint)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) return;

        importer.isReadable = true;

        if (typeHint.Contains("normal"))
        {
            importer.textureType = TextureImporterType.NormalMap;
            importer.sRGBTexture = false;
        }
        else if (typeHint.Contains("metallic") || typeHint.Contains("occlusion"))
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
        }
        else if (typeHint.Contains("emission"))
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
        }
        else
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
        }

        importer.SaveAndReimport();
    }
}
