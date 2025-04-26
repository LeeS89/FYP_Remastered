using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class FindLargestTextures : EditorWindow
{
    private Vector2 scroll;
    private List<(Texture texture, long size)> largestTextures = new();

    [MenuItem("Tools/Analyze/Largest Textures")]
    public static void ShowWindow()
    {
        GetWindow<FindLargestTextures>("Largest Textures");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Scan Textures"))
        {
            ScanTextures();
        }

        if (largestTextures.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Top 20 Largest Textures in Project", EditorStyles.boldLabel);
            scroll = GUILayout.BeginScrollView(scroll);

            foreach (var (texture, size) in largestTextures)
            {
                GUILayout.BeginHorizontal();

                // Texture preview
                EditorGUILayout.ObjectField(texture, typeof(Texture), false);

                // Info
                string info = $"{FormatBytes(size)} | {texture.width}x{texture.height} | {GetFormat(texture)}";
                GUILayout.Label(info, GUILayout.Width(250));

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }

    private void ScanTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture");
        largestTextures.Clear();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (texture != null)
            {
                long size = GetTextureMemorySize(texture);
                largestTextures.Add((texture, size));
            }
        }

        largestTextures = largestTextures
            .OrderByDescending(entry => entry.size)
            .Take(20)
            .ToList();
    }

    private string FormatBytes(long bytes)
    {
        if (bytes > 1_000_000)
            return $"{(bytes / 1_000_000f):F2} MB";
        if (bytes > 1000)
            return $"{(bytes / 1000f):F2} KB";
        return $"{bytes} B";
    }

    private string GetFormat(Texture texture)
    {
        TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
        return importer != null ? importer.textureCompression.ToString() : "Unknown";
    }

    private static long GetTextureMemorySize(Texture texture)
    {
        var method = typeof(Editor).Assembly
            .GetType("UnityEditor.TextureUtil")
            ?.GetMethod("GetStorageMemorySizeLong", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        if (method != null)
        {
            return (long)method.Invoke(null, new object[] { texture });
        }

        Debug.LogWarning("Failed to get texture size via reflection.");
        return 0;
    }
}
