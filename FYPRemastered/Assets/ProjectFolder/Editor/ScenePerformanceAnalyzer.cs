using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class ScenePerformanceAnalyzer : EditorWindow
{
    private List<string> _report = new();

    [MenuItem("Tools/Analyze/Scene Performance Report")]
    public static void ShowWindow()
    {
        GetWindow<ScenePerformanceAnalyzer>("Scene Performance Analyzer");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Analyze Current Scene"))
        {
            AnalyzeScene();
        }

        if (_report.Count > 0)
        {
            GUILayout.Label("⚠ Performance Warnings", EditorStyles.boldLabel);
            foreach (var line in _report)
            {
                EditorGUILayout.HelpBox(line, MessageType.Warning);
            }
        }
    }

    private void AnalyzeScene()
    {
        _report.Clear();

        var renderers = GameObject.FindObjectsOfType<MeshRenderer>();
        var filters = GameObject.FindObjectsOfType<MeshFilter>();
        var lights = GameObject.FindObjectsOfType<Light>();
        var textures = new HashSet<Texture>();
        var transparentMats = new HashSet<Material>();

        int highPolyMeshes = 0;

        foreach (var filter in filters)
        {
            if (filter.sharedMesh != null && filter.sharedMesh.vertexCount > 5000)
                highPolyMeshes++;
        }

        if (highPolyMeshes > 50)
            _report.Add($"👎 Scene contains {highPolyMeshes} high-poly meshes (>5000 verts each).");

        int realtimeLights = 0;
        int shadowCasters = 0;

        foreach (var light in lights)
        {
            if (light.type != LightType.Directional && light.shadows != LightShadows.None)
                shadowCasters++;

            if (light.lightmapBakeType == LightmapBakeType.Realtime)
                realtimeLights++;
        }

        if (realtimeLights > 0)
            _report.Add($"💡 {realtimeLights} realtime lights detected — consider baking.");

        if (shadowCasters > 10)
            _report.Add($"🌑 {shadowCasters} lights casting shadows — consider disabling on small/indoor lights.");

        int transparentObjects = 0;

        foreach (var r in renderers)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat != null && mat.shader != null)
                {
                    if (mat.shader.name.ToLower().Contains("transparent") ||
                        mat.renderQueue >= (int)RenderQueue.Transparent)
                    {
                        transparentObjects++;
                        transparentMats.Add(mat);
                    }

                    // Try grab main texture
                    if (mat.HasProperty("_MainTex"))
                    {
                        var tex = mat.mainTexture;
                        if (tex != null) textures.Add(tex);
                    }
                }
            }
        }

        if (transparentObjects > 30)
            _report.Add($"🧼 {transparentObjects} transparent objects found — can cause overdraw in VR.");

        int uncompressed = 0;
        foreach (var tex in textures)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path)) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureCompression == TextureImporterCompression.Uncompressed)
            {
                uncompressed++;
            }
        }

        if (uncompressed > 0)
            _report.Add($"📦 {uncompressed} textures are uncompressed — could hurt memory/performance.");

        int staticBatchObjects = 0;
        foreach (var r in renderers)
        {
            if (GameObjectUtility.AreStaticEditorFlagsSet(r.gameObject, StaticEditorFlags.BatchingStatic))
                staticBatchObjects++;
        }

        _report.Add($"📌 {staticBatchObjects} objects marked Static for batching.");
        _report.Add($"🎯 Scene has {textures.Count} unique textures and {transparentMats.Count} transparent materials.");
    }
}
