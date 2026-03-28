// File: Assets/Editor/URPVariantCoverage.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class URPVariantCoverage
{
    [MenuItem("Tools/VR/Create URP Coverage Scene (Lit/Unlit + Lights)")]
    public static void CreateCoverageScene()
    {
        // New scene with default camera + directional light
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // --- Ensure URP settings cover the variants (EDITOR-ONLY, uses SerializedObject) ---
        var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (rp != null)
        {
            var so = new SerializedObject(rp);
            // NOTE: field names are internal; valid for current URP versions
            var addMode = so.FindProperty("m_AdditionalLightsRenderingMode");   // enum int: 0=Disabled, 1=PerVertex, 2=PerPixel
            var addShadow = so.FindProperty("m_SupportsAdditionalLightShadows");  // bool
            var softShadow = so.FindProperty("m_SoftShadowsSupported");            // bool (older versions)
            if (addMode != null) addMode.intValue = 2;        // PerPixel
            if (addShadow != null) addShadow.boolValue = true;  // additional light shadows ON
            if (softShadow != null) softShadow.boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(rp);
            AssetDatabase.SaveAssets();
            Debug.Log("[Coverage] URP: Additional Lights = PerPixel, Additional Light Shadows = ON, Soft Shadows supported.");
        }
        else
        {
            Debug.LogWarning("[Coverage] No URP asset found via GraphicsSettings.currentRenderPipeline. Please ensure your URP asset is active.");
        }

        // --- Lights ---
        // Main sun (soft shadows)
        var sun = Object.FindObjectOfType<Light>();
        if (!sun)
        {
            sun = new GameObject("Directional Light").AddComponent<Light>();
            sun.type = LightType.Directional;
        }
        sun.transform.rotation = Quaternion.Euler(50, -30, 0);
        sun.shadows = LightShadows.Soft;
        sun.intensity = 1.0f;

        // Additional per-pixel light with soft shadows
        var extra = new GameObject("Additional Light (Spot)").AddComponent<Light>();
        extra.type = LightType.Spot;
        extra.range = 12f;
        extra.spotAngle = 45f;
        extra.transform.position = new Vector3(0, 2.0f, 2.5f);
        extra.transform.rotation = Quaternion.Euler(20f, 180f, 0f);
        extra.intensity = 1.5f;
        extra.shadows = LightShadows.Soft;

        // --- Helpers ---
        GameObject MakeQuad(string name, Material m, Vector3 pos)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = name;
            q.transform.position = pos;
            var mr = q.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mr.receiveShadows = true;
            mr.sharedMaterial = m;
            return q;
        }

        Material Lit(System.Action<Material> setup = null)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) { Debug.LogError("URP/Lit shader not found."); return null; }
            var m = new Material(sh);
            setup?.Invoke(m);
            return m;
        }

        // --- Coverage quads (exercise keywords) ---
        // 1) Opaque
        MakeQuad("Lit_Opaque", Lit(), new Vector3(-4, 0, 3));

        // 2) Normal-mapped
        MakeQuad("Lit_NormalMap", Lit(m =>
        {
            m.EnableKeyword("_NORMALMAP");
            m.SetTexture("_BumpMap", Texture2D.normalTexture);
            m.SetFloat("_BumpScale", 1f);
        }), new Vector3(-2, 0, 3));

        // 3) Emission
        MakeQuad("Lit_Emission", Lit(m =>
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.white * 1.8f);
        }), new Vector3(0, 0, 3));

        // 4) Alpha Clip (cutout)
        MakeQuad("Lit_AlphaClip", Lit(m =>
        {
            m.EnableKeyword("_ALPHATEST_ON");
            m.SetFloat("_AlphaClip", 1f); // URP toggle
            m.SetFloat("_Cutoff", 0.5f);
            // tiny checker alpha texture
            var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.SetPixels(new[]
            {
                new Color(1,1,1,0), new Color(1,1,1,1),
                new Color(1,1,1,1), new Color(1,1,1,0)
            });
            tex.Apply();
            m.SetTexture("_BaseMap", tex);
        }), new Vector3(2, 0, 3));

        // 5) Transparent (blended)
        MakeQuad("Lit_Transparent", Lit(m =>
        {
            m.SetFloat("_Surface", 1f); // 0 Opaque, 1 Transparent
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            var c = m.GetColor("_BaseColor"); c.a = 0.5f; m.SetColor("_BaseColor", c);
            m.SetFloat("_ZWrite", 0f);
        }), new Vector3(4, 0, 3));

        // 6) Unlit
        {
            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            var unlit = new Material(sh);
            MakeQuad("Unlit_Basic", unlit, new Vector3(0, 0, 5));
        }

        // Optional: a static object to quickly register LIGHTMAP_ON once you Auto-Generate lighting
        var lm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lm.name = "Lightmapped_Cube (mark Static then Auto Generate lighting)";
        lm.transform.position = new Vector3(-6, 0, 3);
        GameObjectUtility.SetStaticEditorFlags(lm, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorGUIUtility.PingObject(Camera.main);

        EditorUtility.DisplayDialog(
            "Coverage Scene Ready",
            "Scene created with main (soft shadows) + additional spot (soft shadows) and Lit/Unlit quads.\n\n" +
            "Next:\n" +
            "1) (Optional) In Lighting window, tick 'Auto Generate' once to bake (captures LIGHTMAP_ON).\n" +
            "2) Open Project Settings > Graphics > Preloaded Shaders and click 'Save to asset…' to write a Shader Variant Collection.\n" +
            "3) Add that .shadervariants to Preloaded Shaders and call WarmUp() at boot.",
            "OK");
    }
}
#endif
