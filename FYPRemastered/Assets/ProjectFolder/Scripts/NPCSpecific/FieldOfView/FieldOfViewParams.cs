using UnityEngine;

[System.Serializable]
public class FieldOfViewParams
{
    [Header("Targeting phase origin - Final phase of FOV check, Linecast from eyes.\n" +
        "Also the origin for initial Detection phase, uses OverlapSphere from this origin")]
    public Transform fovOrigin;
    public float fovAngle = 50.0f;
    public int maxFovTargets = 5;
    public LayerMask blockingMask;
    public LayerMask targetMask;
    [Header("Radius for detection phase - also the max distance in evaluation phase")]
    public float fovRadius = 25.0f;

    [Header("Evaluation Phase params - Uses capsule cast from owner origin + waist and eye height offsets\n" +
        "Gathers targets for the final targeting phase")]
    public Transform ownerOrigin;
    public float waistHeightOffset = 1.0f;
    public float eyeHeightOffset = 1.8f;
    public float evaluationCapsuleRadius = 0.4f;

    [Header("When true, adds extra points on detected target colliders\n" +
        "from the evaluation phase to use in the targeting phase\n" +
        "increasing robustness of FOV check - may impact performance")]
    public bool addTargetFallbackPoints = true;

    [Header("Optional - Ensures ranged weapons can only be used once fully aiming in the targets direction\n" +
        "within halfHorizontalShootAngle threshold")]
    public bool useShootingAngleRestriction = true;
    public float halfHorizontalShootAngle = 15.0f;

    [Header("Frequency of FOV checks when target is outide of fovRadius")]
    public float idleFOVCheckFrequency = 1f;

    [Header("Frequency of FOV checks upon either losing LOS to target after some time\n" +
        "or some other cue such as hearing a noise")]
    public float suspiciousFOVCheckFrequency = 0.5f;

    [Header("Frequency of FOV checks when alerted to target")]
    public float alertedFOVCheckFrequency = 0.1f;
}
