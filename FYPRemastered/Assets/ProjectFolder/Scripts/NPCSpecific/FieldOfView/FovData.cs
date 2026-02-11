using System.Text;
using UnityEngine;

[System.Serializable]
public class FovData : IFovDeps
{
    [Header("Targeting phase origin - Final phase of FOV check, Linecast from eyes.\n" +
       "Also the origin for initial Detection phase, uses OverlapSphere from this origin")]
    public Transform fovOrigin;
    public int maxFovTargets = 5;
    public LayerMask blockingMask; // Make protected and use methods to modify? => Obsolete
    // Plus, modify dynamically depending on FOV phase

    //public LayerMask NpcLayer;
    public LayerMask worldMask;

    private bool _targetInRadius = false;
    public ITargetable Target { get; private set; }
    private AlertPhase _currentPhase = AlertPhase.Idle;

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
        /*if(Target != null)
            blockingMask &= ~Target.LayerMask;*/ // Remove previous target layer from blocking mask

      //  CoroutineRunner.Instance.StartCoroutine(TestPruint());
       
        Target = target;
       // blockingMask |= Target.LayerMask;
    }

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
    public float waistHeightOffset = 1.0f; // Obsolete
    public float eyeHeightOffset = 1.8f; // Obsolete
    public float evaluationCapsuleRadius = 0.4f; // Obsolete

    [Header("When true, adds extra points on detected target colliders\n" +
        "from the evaluation phase to use in the targeting phase\n" +
        "increasing robustness of FOV check - may impact performance")]
    public bool addTargetFallbackPoints = false; // Obsolete??

    [Header("The angle from fovOrigin.forward which target mucst be within\n" +
        "for a successful LOS hit")]
    public float fovHalfAngle = 50.0f;

/*    [Header("If false, uses fovHalfAngle for both H and V angle\n" +
        "If true, uses fovHalfAngle for Horizontal check")]
    public bool useSeparateVerticleAngle = false;
    public float verticalFovHalfAngle = 25f;
*/
    [Header("Optional - Ensures ranged weapons can only be used once fully aiming in the targets direction\n" +
        "within halfHorizontalShootAngle threshold")]
    public bool useShootingAngleRestriction = true;
    public float halfShootAngle = 15.0f;

    [Header("Frequency of FOV checks when target is outside of fovRadius")]
    public float idleFOVCheckFrequency = 0.5f;

   /* [Header("Frequency of FOV checks when target is inside of fovRadius\n" +
        "but without any alerted or suspicious cues")]
    public float heightenedFOVCheckFrequency = 0.5f;*/

    [Header("Frequency of FOV checks upon either losing LOS to target after some time\n" +
        "or some other cue such as hearing a noise")]
    public float suspiciousFOVCheckFrequency = 0.25f;

    [Header("Frequency of FOV checks when alerted to target")]
    public float alertedFOVCheckFrequency = 0.1f;

  
    public float GetSweepFrequency()
    {
        return _currentPhase switch
        {
            AlertPhase.Idle => _targetInRadius ? idleFOVCheckFrequency : (idleFOVCheckFrequency * 2f),
            //AlertPhase.Heightened => heightenedFOVCheckFrequency,
            AlertPhase.Suspicious => _targetInRadius ? suspiciousFOVCheckFrequency : (suspiciousFOVCheckFrequency * 2f),
            AlertPhase.Alerted => _targetInRadius ? alertedFOVCheckFrequency : (alertedFOVCheckFrequency * 2f),
            _ => idleFOVCheckFrequency
        };
    }

    public void SetAlertPhase(AlertPhase phase) => _currentPhase = phase;

    public LayerMask WorldMask() => worldMask;
    public LayerMask BlockingMask() => blockingMask;
    public Transform OwnerOrigin() => ownerOrigin;
    public Transform SweepOrigin() => fovOrigin;
    public float FovRadius() => fovRadius;
    public float FovHalfAngle() => fovHalfAngle;
    public int MaxTargets() => maxFovTargets;
    public bool UseShootingAngleRestriction() => useShootingAngleRestriction;
    public float HalfShootAngle() => halfShootAngle;

    public void SetTargetProximityStatus(bool targetInsideRadius) => _targetInRadius = targetInsideRadius;

    public void DebugFrequency()
    {
        Debug.LogError("Current AlertPhase is: "+_currentPhase.ToString() + ": and time is: "+GetSweepFrequency());
    }
}


