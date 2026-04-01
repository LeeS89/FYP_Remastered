using UnityEngine;

[CreateAssetMenu(fileName = "CueDef", menuName = "Abilities/Cue")]
public class CueDef : ScriptableObject
{
    [Header("Sound")]
    public AudioClip Sound;
    public float Volume = 1f;

    [Header("Visuals")]
    public GameObject VfxPrefab;
    public Vector3 VfxOffset;
    public float VfxLifetime = 2f;

    [Header("Animation")]
    public string AnimationTrigger;

    [Header("Camera/ Screen FX")]
    public bool CameraShake;
    public float ShakeIntensity;
    public float ShakeDuration;
}
