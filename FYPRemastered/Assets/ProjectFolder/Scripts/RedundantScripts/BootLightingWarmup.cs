using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BootLightingWarmup : MonoBehaviour
{
    [Tooltip("Optional: Materials to touch. Leave empty to auto-generate basic Lit/Unlit materials.")]
    public List<Material> materials;

    [Tooltip("URP Renderer index (usually 0 unless you use multiple renderers).")]
    public int rendererIndex = 0;

    IEnumerator Start()
    {
        // Build fallback materials if user didn't assign any
        if (materials == null || materials.Count == 0)
        {
            materials = new List<Material>();
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit) materials.Add(new Material(lit));
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlit) materials.Add(new Material(unlit));
        }

        // Create tiny RT and hidden camera
        var rt = new RenderTexture(128, 128, 0);
        var camGO = new GameObject("WarmCam (Temp)");
        var cam = camGO.AddComponent<Camera>();
        cam.enabled = false;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.targetTexture = rt;

        var add = cam.GetUniversalAdditionalCameraData();
        add.SetRenderer(rendererIndex); // match your gameplay renderer

        // Lights: main dir + additional spot (soft shadows)
        var sun = new GameObject("Warm Sun").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50, -30, 0);

        var spot = new GameObject("Warm Spot").AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.shadows = LightShadows.Soft;
        spot.range = 10f;
        spot.spotAngle = 45f;
        spot.transform.position = new Vector3(0, 1.2f, 2.2f);
        spot.transform.rotation = Quaternion.Euler(20, 180, 0);

        // Quad to render the materials
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.position = new Vector3(0, 0, 2.0f);

        foreach (var m in materials)
        {
            if (!m) continue;
            quad.GetComponent<MeshRenderer>().sharedMaterial = m;
            yield return null;  // give Unity one frame to upload textures/meshes
            cam.Render();       // actually renders → forces PSO compilation + GPU driver warm-up
        }

        // Cleanup
        Destroy(rt);
        Destroy(camGO);
        Destroy(sun.gameObject);
        Destroy(spot.gameObject);
        Destroy(quad);

        // Optional: trigger GC so first GC doesn't happen mid-gameplay
        System.GC.Collect();
    }
}
