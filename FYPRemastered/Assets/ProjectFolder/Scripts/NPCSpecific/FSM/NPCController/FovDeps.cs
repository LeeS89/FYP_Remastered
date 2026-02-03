using System;
using System.Collections;
using System.Text;
using UnityEngine;

[System.Serializable]
public class FovDeps
{
    [Header("Targeting phase origin - Final phase of FOV check, Linecast from eyes.\n" +
       "Also the origin for initial Detection phase, uses OverlapSphere from this origin")]
    public Transform fovOrigin;
    public int maxFovTargets = 5;
    public LayerMask blockingMask; // Make protected and use methods to modify? => Obsolete
    // Plus, modify dynamically depending on FOV phase

    //public LayerMask NpcLayer;
    public LayerMask worldLayers;

  
    public ITargetable Target { get; private set; }

    public void SetTarget(ITargetable target)
    {
        if (target == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Must provide a valid target for FOV sweeps");
#endif
            return;
        }
        
        if (target == Target) return;
        if(Target != null)
            blockingMask &= ~Target.LayerMask; // Remove previous target layer from blocking mask

      //  CoroutineRunner.Instance.StartCoroutine(TestPruint());
       
        Target = target;
        blockingMask |= Target.LayerMask;
    }


   /* IEnumerator TestPruint()
    {
        yield return new WaitForSeconds(5f);
        int losMask = worldLayers.value | NpcLayer.value;
      //  LayerMask newMask = worldLayers |= NpcLayer;
        Debug.LogError(DescribeLayers(losMask));
    }*/

    public string DescribeLayers(LayerMask mask)
    {
        int bits = mask.value;
        var sb = new StringBuilder();

        sb.Append("LayerMask (value=").Append(bits).Append(") contains: ");

        bool any = false;
        for (int layer = 0; layer < 32; layer++)
        {
            if ((bits & (1 << layer)) != 0)
            {
                string name = LayerMask.LayerToName(layer);
                if (string.IsNullOrEmpty(name)) name = $"<unnamed:{layer}>";

                if (any) sb.Append(", ");
                sb.Append(name);
                any = true;
            }
        }

        if (!any) sb.Append("<none>");
        return sb.ToString();
    }

    public LayerMask TargetMask() => Target == null ? default : Target.LayerMask;
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
    public bool addTargetFallbackPoints = false;

    [Header("The angle from fovOrigin.forward which target mucst be within\n" +
        "for a successful LOS hit")]
    public float fovHalfAngle = 50.0f;

    [Header("If false, uses fovHalfAngle for both H and V angle\n" +
        "If true, uses fovHalfAngle for Horizontal check")]
    public bool useSeparateVerticleAngle = false;
    public float verticalFovHalfAngle = 25f;

    [Header("Optional - Ensures ranged weapons can only be used once fully aiming in the targets direction\n" +
        "within halfHorizontalShootAngle threshold")]
    public bool useShootingAngleRestriction = true;
    public float halfHorizontalShootAngle = 15.0f;

    [Header("Frequency of FOV checks when target is outide of fovRadius")]
    public float idleFOVCheckFrequency = 1f;

    [Header("Frequency of FOV checks when target is inside of fovRadius\n" +
        "but without any alerted or suspicious cues")]
    public float heightenedFOVCheckFrequency = 0.5f;

    [Header("Frequency of FOV checks upon either losing LOS to target after some time\n" +
        "or some other cue such as hearing a noise")]
    public float suspiciousFOVCheckFrequency = 0.25f;

    [Header("Frequency of FOV checks when alerted to target")]
    public float alertedFOVCheckFrequency = 0.1f;

    [Header("How long to wait after LOS has been lost to downgrade alert status")]
    public float alertToSuspiciousDelaySeconds = 3f;
    public float suspiciousToIdleDelaySeconds = 5f;

    [Header("Callbacks")]
    public Action<bool, bool> OnFOVSweepResult;
}
