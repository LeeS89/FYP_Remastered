using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BootWarmupOrchestrator : MonoBehaviour
{
    [Header("Shaders")]
    public ShaderVariantCollection shaderVariants;   // optional; call WarmUp()

    [Header("Renderer / Materials")]
    public int urpRendererIndex = 0;                 // match your gameplay camera's renderer
    public List<Material> materials;                 // optional; if empty, Lit/Unlit will be auto-created

    [Header("Post-processing")]
    public List<Volume> postVolumesToWarm;           // optional; will be enabled briefly

    [Header("Overlay/Additional Cameras")]
    public List<Camera> camerasToWarm;               // e.g., UI overlay cam; we render once offscreen

    [Header("RenderTextures to Precreate")]
    public List<RenderTexture> renderTexturesToWarm; // if you allocate RTs at runtime, assign refs here

    [Header("Skinned / Animator / IK")]
    public List<Animator> animatorsToWarm;           // enemies/NPCs prefabs already in scene or temp
    public bool primeIK = true;                      // run OnAnimatorIK paths with tiny weight if present

    [Header("Prefabs / Pools")]
    public List<GameObject> prefabsToWarm;           // instantiate once, SetActive true one frame, then destroy

    [Header("Audio")]
    public List<AudioClip> sfxToWarm;                // small SFX; will be played at 0 volume
    public List<AudioClip> musicToWarm;              // long clips; start/stop once
    public AudioMixerGroup mutedGroup;               // optional; route to muted mixer
    public float audioSilenceVolume = 0f;

    [Header("Occlusion pre-touch")]
    public List<Transform> occlusionProbeCameras;    // positions/rotations to render from (hidden)
    public int occlusionProbeRendererIndex = 0;

    [Header("Options")]
    public int offscreenSize = 128;                  // tiny RT size for warm renders
    public bool forceGCCollectAtEnd = true;

    IEnumerator Start()
    {
        // 1) Shader Variants
        if (shaderVariants && !shaderVariants.isWarmedUp)
            shaderVariants.WarmUp();  // synchronous; OK in boot scene

        // 2) Build fallback materials if none provided
        if (materials == null || materials.Count == 0)
        {
            materials = new List<Material>();
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (lit) materials.Add(new Material(lit));
            if (unlit) materials.Add(new Material(unlit));
        }

        // 3) Offscreen render under main + additional light with soft shadows (hits PSOs)
        yield return StartCoroutine(WarmLightingAndMaterials(materials, urpRendererIndex, offscreenSize));

        // 4) Post-processing volumes (enable one frame)
        if (postVolumesToWarm != null)
            foreach (var v in postVolumesToWarm)
                if (v) yield return StartCoroutine(WarmPostVolume(v));

        // 5) Ensure RTs & overlay cameras touch their paths
        if (renderTexturesToWarm != null)
            foreach (var rt in renderTexturesToWarm)
                if (rt && !rt.IsCreated()) rt.Create();

        if (camerasToWarm != null)
            foreach (var c in camerasToWarm)
                if (c) yield return StartCoroutine(RenderOnceOffscreen(c, offscreenSize));

        // 6) Skinned/Animator/IK first-use
        if (animatorsToWarm != null)
            foreach (var a in animatorsToWarm)
                if (a) yield return StartCoroutine(PrimeAnimator(a, primeIK));

        // 7) Prefabs/Pools (touch once)
        if (prefabsToWarm != null)
            foreach (var prefab in prefabsToWarm)
                if (prefab) yield return StartCoroutine(TouchPrefab(prefab));

        // 8) Audio warm-up
        yield return StartCoroutine(WarmAudio(sfxToWarm, musicToWarm));

        // 9) Occlusion pre-touch (renders tiny frame at those camera poses)
        if (occlusionProbeCameras != null)
            foreach (var t in occlusionProbeCameras)
                if (t) yield return StartCoroutine(PreTouchArea(t.position, t.rotation, occlusionProbeRendererIndex, offscreenSize));

        // 10) Optional GC before gameplay
        if (forceGCCollectAtEnd) System.GC.Collect();
    }

    // ---- Helpers ----

    IEnumerator WarmLightingAndMaterials(List<Material> mats, int rendererIndex, int size)
    {
        var rt = new RenderTexture(size, size, 0);
        var camGO = new GameObject("WarmCam(LitPSO)");
        var cam = camGO.AddComponent<Camera>();
        cam.enabled = false; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
        cam.targetTexture = rt;
        var add = cam.GetUniversalAdditionalCameraData(); add.SetRenderer(rendererIndex);

        // Main directional + additional spot (soft shadows)
        var sun = new GameObject("Warm Sun").AddComponent<Light>();
        sun.type = LightType.Directional; sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50, -30, 0);

        var spot = new GameObject("Warm Spot").AddComponent<Light>();
        spot.type = LightType.Spot; spot.shadows = LightShadows.Soft; spot.range = 10f; spot.spotAngle = 45f;
        spot.transform.position = new Vector3(0, 1.2f, 2.2f); spot.transform.rotation = Quaternion.Euler(20, 180, 0);

        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.position = new Vector3(0, 0, 2.0f);
        var mr = quad.GetComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        mr.receiveShadows = true;

        foreach (var m in mats)
        {
            if (!m) continue;
            mr.sharedMaterial = m;
            yield return null;   // allow uploads
            cam.Render();        // force PSO compile under lit+shadows
        }

        Destroy(rt); Destroy(camGO); Destroy(sun.gameObject); Destroy(spot.gameObject); Destroy(quad);
    }

    IEnumerator WarmPostVolume(Volume v)
    {
        if (!v) yield break;
        bool had = v.enabled;
        v.enabled = true;
        yield return null;      // let the stack initialize
        v.enabled = had;
    }

    IEnumerator RenderOnceOffscreen(Camera srcCam, int size)
    {
        // clone minimal to avoid side effects
        var rt = new RenderTexture(size, size, 0);
        var camGO = new GameObject("WarmCam(Overlay)");
        var cam = camGO.AddComponent<Camera>();
        cam.CopyFrom(srcCam);
        cam.enabled = false; cam.targetTexture = rt;
        yield return null;
        cam.Render();
        Destroy(rt); Destroy(camGO);
    }

    IEnumerator PrimeAnimator(Animator a, bool doIK)
    {
        // AlwaysAnimate so evaluation happens even if offscreen
        var old = a.cullingMode;
        a.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // tiny IK weight via parameter if you use it; otherwise OnAnimatorIK can gate by Time.frameCount
        if (doIK && a.isInitialized)
        {
            // optional: if you have a param that gates IK, set it here
        }

        a.Update(0f);
        yield return null;
        a.Update(0f);

        a.cullingMode = old;
    }

    IEnumerator TouchPrefab(GameObject prefab)
    {
        var go = Instantiate(prefab);
        go.SetActive(true);
        yield return null; // materials/anim/skin upload
        go.SetActive(false);
        Destroy(go);
    }

    IEnumerator WarmAudio(List<AudioClip> sfx, List<AudioClip> music)
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.volume = audioSilenceVolume;
        if (mutedGroup) src.outputAudioMixerGroup = mutedGroup;

        if (sfx != null)
        {
            foreach (var clip in sfx)
            {
                if (!clip) continue;
                src.PlayOneShot(clip);
                // no wait needed; decode kicks off
            }
        }

        if (music != null)
        {
            foreach (var clip in music)
            {
                if (!clip) continue;
                src.clip = clip; src.loop = false;
                src.Play();
                yield return null; // let streaming/decode start
                src.Stop();
            }
        }
    }

    IEnumerator PreTouchArea(Vector3 pos, Quaternion rot, int rendererIndex, int size)
    {
        var rt = new RenderTexture(size, size, 0);
        var go = new GameObject("PreTouchCam");
        var cam = go.AddComponent<Camera>();
        cam.enabled = false; cam.targetTexture = rt; cam.clearFlags = CameraClearFlags.SolidColor;
        var add = cam.GetUniversalAdditionalCameraData(); add.SetRenderer(rendererIndex);
        go.transform.SetPositionAndRotation(pos, rot);
        yield return null;
        cam.Render(); // triggers visibility/PSO/texture uploads for that occlusion cell
        Destroy(rt); Destroy(go);
    }
}
