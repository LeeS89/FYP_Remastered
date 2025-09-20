// File: Assets/Editor/SceneShaderVariantCollector.cs
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class SceneShaderVariantCollector
{
    // Common pass types to try; SRP ignores some, but it's safe to add.
    static readonly PassType[] PassesToTry = new[]
    {
        PassType.ScriptableRenderPipeline, // SRP default path
        PassType.ForwardBase,              // legacy forward (harmless to include)
        PassType.ForwardAdd,
        PassType.ShadowCaster,
        PassType.Meta,                     // lightmapping/GI
       /* PassType.DepthNormals,
        PassType.DepthOnly*/
    };

    [MenuItem("Tools/Shaders/Create SVC from Scene (New Asset)")]
    public static void CreateSvcFromScene()
    {
        var path = EditorUtility.SaveFilePanelInProject(
            "Save Shader Variant Collection",
            "SceneVariants",
            "shadervariants",
            "Choose a location for the new Shader Variant Collection asset.");
        if (string.IsNullOrEmpty(path)) return;

        var svc = new ShaderVariantCollection();
        AssetDatabase.CreateAsset(svc, path);
        AssetDatabase.SaveAssets();

        var report = CollectAndAddSceneVariants(svc);
        EditorUtility.SetDirty(svc);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("SVC Created",
            $"Shaders found: {report.shaders}\nVariants added: {report.variants}\nDuplicates skipped: {report.duplicates}",
            "OK");
        Selection.activeObject = svc;
    }

    [MenuItem("Tools/Shaders/Append Scene Variants to Selected SVC")]
    public static void AppendToSelectedSvc()
    {
        var svc = Selection.activeObject as ShaderVariantCollection;
        if (svc == null)
        {
            EditorUtility.DisplayDialog("No SVC Selected",
                "Select an existing ShaderVariantCollection asset in the Project and run this again.",
                "OK");
            return;
        }

        var report = CollectAndAddSceneVariants(svc);
        EditorUtility.SetDirty(svc);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("SVC Updated",
            $"Shaders found: {report.shaders}\nVariants added: {report.variants}\nDuplicates skipped: {report.duplicates}",
            "OK");
        Selection.activeObject = svc;
    }

    struct Report { public int shaders, variants, duplicates; }

    static Report CollectAndAddSceneVariants(ShaderVariantCollection svc)
    {
        var report = new Report();
        var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // Key is shader + sorted keyword list string
        var seen = new HashSet<string>();
        var shadersInScene = new HashSet<Shader>();

        foreach (var r in renderers)
        {
            if (r == null) continue;
            var mats = r.sharedMaterials;
            if (mats == null) continue;

            foreach (var mat in mats)
            {
                if (mat == null || mat.shader == null) continue;

                shadersInScene.Add(mat.shader);

                var keywords = GetEnabledKeywordNames(mat);
                Array.Sort(keywords, StringComparer.Ordinal);

                var keyStr = $"{mat.shader.name}::{string.Join("|", keywords)}";
                if (!seen.Add(keyStr))
                {
                    report.duplicates++;
                    continue;
                }

                // Try multiple pass types; SRP will effectively focus on SRP path.
                foreach (var pass in PassesToTry)
                {
                    try
                    {
                        var variant = new ShaderVariantCollection.ShaderVariant(mat.shader, pass, keywords);
                        svc.Add(variant);
                        report.variants++;
                    }
                    catch
                    {
                        // Some shaders/passes may throw; ignore and continue.
                    }
                }
            }
        }

        report.shaders = shadersInScene.Count;
        Debug.Log($"[SVC Collector] Scene scan complete. Shaders: {report.shaders}, Unique material keyword sets: {seen.Count}, Variants added x passes: {report.variants}, Duplicates: {report.duplicates}");
        return report;
    }

    // Extract enabled keyword names in a version-tolerant way
    static string[] GetEnabledKeywordNames(Material mat)
    {
        // Newer Unity: LocalKeyword API
#if UNITY_2021_2_OR_NEWER
        try
        {
            var kws = mat.enabledKeywords;
            if (kws != null && kws.Length > 0)
                return kws.Select(k => k.name).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToArray();
        }
        catch { /* fall back below */ }
#endif
        // Older API
#pragma warning disable CS0618
        var legacy = mat.shaderKeywords;
#pragma warning restore CS0618
        if (legacy != null && legacy.Length > 0)
            return legacy.Distinct().ToArray();

        // No keywords enabled = base variant
        return Array.Empty<string>();
    }
}
#endif
