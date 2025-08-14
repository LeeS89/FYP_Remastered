using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class CombatComponent : BaseAbilities
{
    protected EnemyEventManager _enemyEventManager;

    [Header ("Field of view Origin")]
    [SerializeField] public Transform _fovLocation;
    [Header("Max Field of view targets")]
    [SerializeField] private int _maxFovTraceResults = 5;
    [Header("Field of view proximity phase radius")]
    [SerializeField] public float _proximityRadius = 5f; // Make Protected later

    [Header("Field of view evaluation phase")]
    [Header("start and end points + radius of capsule cast in FOV evaluation phase")]
    [SerializeField] private float _waistHeight = 1.0f;
    [SerializeField] private float _eyeHeight = 1.8f;
    [SerializeField] private float _fovEvaluationRadius = 0.4f;

    [Header ("Field of view angle with horizontal & vertical multipliers")]
    [Range(0, 360)] public float _fovViewangle;
    [Range(0, 2)] public float _horizontalAngleMultiplier;
    [Range(0, 2)] public float _verticalAngleMultiplier;
    

    [Header ("Field of view region - Melee target, FOV obstruction, and FOV target masks")]
    [SerializeField] private LayerMask _meleeCheckMask;
    [SerializeField] private LayerMask _fovTargetMask;
    [SerializeField] private LayerMask _fovBlockingMask;

    [Header ("Melee attack check interval")]
    [SerializeField] private float _meleeCheckInterval = 0.2f;

    [Header ("Melee trigger radius")]
    [SerializeField] private float _meleeCheckRadius = 1.5f;
    protected bool _meleeTriggered = false;
    private Coroutine _meleeCheckCoroutine;
    private WaitForSeconds _meleeCheckWait;
    private bool _evaluatingMeleeCheck = false;

   

    [Range(0, 360)] public float _shootAngleThreshold;


    private AITraceComponent _aiTraceComponent;
    
  
    [SerializeField] private Collider[] _meleeResults;

    private Action<bool, bool> _fovCallback;



    private FieldOfViewHandler _fovhandler;
    private FieldOfViewParams _fovParams;
    

    AgentWeaponHandler _weaponHandler;
    //[SerializeField] protected bool _targetSeen = false;

    private void OwnerDeathStatusChanged(bool isDead)
    {
        IsOwnerDead = isDead;

        if (isDead)
        {
            OnFieldOfViewComplete(false, false);
            _weaponHandler?.UnEquipWeapon();
        }
        else
        {
            _weaponHandler?.EquipWeapon(WeaponType.Ranged);
        }
    }
 
    public bool IsOwnerDead { get; protected set; }

   /* public bool IsTargetDead { get; protected set; }

    protected void TargetDeath() => IsTargetDead = true;
    protected void TargetRespawn() => IsTargetDead = false;*/

    protected void SetMeleeTriggered(bool isMelee) => _meleeTriggered = isMelee;

  

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _enemyEventManager = eventManager as EnemyEventManager;
        base.RegisterLocalEvents(_enemyEventManager);

        _aiTraceComponent = new AITraceComponent();
      
        _meleeResults = new Collider[2];
      
        _meleeCheckWait = new WaitForSeconds(_meleeCheckInterval);
        _fovCallback = OnFieldOfViewComplete;

        InitializeFOVParams();
        _fovhandler = new FieldOfViewHandler(_aiTraceComponent, _enemyEventManager, _fovParams, true);

        _enemyEventManager.OnFieldOfViewCallback += OnFieldOfViewComplete;

        _enemyEventManager.OnOwnerDeathStatusUpdated += OwnerDeathStatusChanged;
     

        _enemyEventManager.OnMeleeAttackPerformed += EvaluateMeleeAttackResults;
   
        _enemyEventManager.OnMelee += SetMeleeTriggered;

        Transform player = GameManager.Instance.GetPlayerPosition(PlayerPart.DefenceCollider);

        _weaponHandler = new AgentWeaponHandler(_enemyEventManager, gameObject, _bulletSpawnPoint, player, 20);

        RegisterGlobalEvents();
       
    }

   

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _meleeCheckWait = null;
    
        _meleeResults = null;
        _enemyEventManager.OnMeleeAttackPerformed -= EvaluateMeleeAttackResults;
    
        _enemyEventManager.OnMelee -= SetMeleeTriggered;
        _enemyEventManager.OnOwnerDeathStatusUpdated -= OwnerDeathStatusChanged;
        _enemyEventManager.OnFieldOfViewCallback -= OnFieldOfViewComplete;

        base.UnRegisterLocalEvents(_enemyEventManager);
    
    }

    protected override void RegisterGlobalEvents()
    {
        base.RegisterGlobalEvents();
        BaseSceneManager._instance.OnSceneStarted += OnSceneStarted;
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;
       

    }
    protected override void UnRegisterGlobalEvents()
    {
        base.UnRegisterGlobalEvents();
        BaseSceneManager._instance.OnSceneStarted -= OnSceneStarted;
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;
        
    }


    public Transform _bulletSpawnPoint;
    private void InitializeFOVParams()
    {
        _fovParams = new FieldOfViewParams
        (
            _fovLocation,
            transform,
            _bulletSpawnPoint,
            _proximityRadius,
            _waistHeight,
            _eyeHeight,
            _fovEvaluationRadius,
            _fovViewangle * _horizontalAngleMultiplier,
            _fovViewangle * _verticalAngleMultiplier,
            _shootAngleThreshold * 0.5f,
            _shootAngleThreshold * 1.25f,
            _fovBlockingMask,
            _fovTargetMask,
            _maxFovTraceResults

        );
    }

    public bool _testMelee = false;

    private void LateUpdate()
    {
        if (IsOwnerDead || PlayerIsDead) { return; }

        if (_testMelee)
        {
            _evaluatingMeleeCheck = true;
            ToggleMeleeCheckRoutine(true);
            _testMelee = false;
        }

        FieldOfViewEvaluation();
        UpdateWeaponHandler();
       
    }

    private void UpdateWeaponHandler()
    {
        if (_weaponHandler == null /*|| !_canUpdateWeapon*/) { return; }

        _weaponHandler.UpdateEquippedWeapon();
    }

    private bool _updateFOV = false;

    private void FieldOfViewEvaluation()
    {
        if (!_updateFOV || _fovhandler == null) { return; }

        _fovhandler.UpdateFieldOfView();


       /* if (Time.time >= _nextShootTime)
        {
            CheckFieldOfView();
            *//* if (CanShoot())
             {
                 _enemyEventManager.AnimationTriggered(AnimationAction.Shoot);
             }*//*
            //_weapon.TryFireRangedWeapon();
            _nextShootTime = Time.time + _shootInterval;
        }*/
    }

    protected override void OnPlayerDeathStatusUpdated(bool isDead)
    {
        base.OnPlayerDeathStatusUpdated(isDead);
        if (PlayerIsDead)
        {
            _enemyEventManager.TargetSeen(false);
            _enemyEventManager.FacingTarget(false);
            
            ResetMeleeSweep();
        }
        
    }

    public bool _canUpdateWeapon = false;


    public static bool _testFOV = true;

    private void BeginMeleeSweep()
    {
        ToggleMeleeCheckRoutine(true);
    }

    private void ResetMeleeSweep()
    {
        ToggleMeleeCheckRoutine(false);
    }

    private void ToggleMeleeCheckRoutine(bool conditionMet)
    {
        if (conditionMet)
        {
            if (_meleeCheckCoroutine == null)
            {
                _meleeCheckCoroutine = StartCoroutine(MeleeCheckRoutine());
            }
        }
        else
        {
            if (_meleeCheckCoroutine != null)
            {
                StopCoroutine(_meleeCheckCoroutine);
                _meleeCheckCoroutine = null;
            }
        }
    }

    private IEnumerator MeleeCheckRoutine()
    {
        while (_evaluatingMeleeCheck)
        {
            //int numTargetsDetected = _aiTraceComponent.CheckTargetProximity(_fovLocation, _fovTraceResults, _meleeCheckRadius, _meleeCheckMask, true);

            if (_aiTraceComponent.IsTargetWithinRange(_fovLocation.position, _meleeCheckRadius, _meleeCheckMask, true))
            {
                PerformMeleeAttack();
                yield return new WaitForSeconds(1.5f); // Delay to allow melee animation to play
            }
            _enemyEventManager.PursuitConditionChanged(true);
            yield return _meleeCheckWait;
        }
    }

    private void PerformMeleeAttack()
    {
        _enemyEventManager.PursuitConditionChanged(false);
        _enemyEventManager.AnimationTriggered(AnimationAction.Melee);
    }

    private void EvaluateMeleeAttackResults()
    {
        int targets = _aiTraceComponent.CheckTargetWithinCombatRange(_fovLocation.position, _meleeResults, _meleeCheckRadius, _meleeCheckMask);

        if(targets == 0) { return; }
        
        foreach (var target in _meleeResults)
        {
            if (target == null)
                continue;

            if (target.gameObject.TryGetComponent(out IDamageable damageable))
            {
                damageable.Knockback(500, transform.forward, 10f, 0.3f);
                break;
            }
        }

        Array.Clear(_meleeResults, 0, _meleeResults.Length);
    }

    public override void GunSetup(GameObject owner, EventManager eventManager, Transform bulletSpawnLocaiton, int clipCapacity, Transform target)
    {
        //_weapon = new WeaponHandlerBase();

        _weaponHandler.EquipWeapon(owner, _enemyEventManager , bulletSpawnLocaiton, clipCapacity, target);
        // base.GunSetup(owner, _enemyEventManager, bulletSpawnLocaiton, clipCapacity, target);
    }



    protected override void OnSceneStarted()
    {
        _canUpdateWeapon = true;
        _updateFOV = true;
    }


    public bool _testSeen = false;

    private void OnFieldOfViewComplete(bool seen, bool inShootingAngle)
    {
        if (!seen && _testFOV)
        {
           // Debug.LogError("Cannot see player On FOV callback");
        }
        _enemyEventManager.TargetSeen(seen);
        _enemyEventManager.FacingTarget(inShootingAngle);
        //SetFacingtarget(inShootingAngle);
    }

    protected void CheckFieldOfView()
    {
        if(_fovhandler == null || _aiTraceComponent == null) { return; }
        _fovhandler.RunFieldOfViewSweep(/*OnFieldOfViewComplete, true*/);
    }


    void OnDrawGizmosSelected()
    {
        if (_fovLocation == null) return;

        Vector3 origin = _fovLocation.position;
        float viewRadius = _proximityRadius;
        float hAng = _fovViewangle * _horizontalAngleMultiplier;
        float vAng = _fovViewangle * _verticalAngleMultiplier;

#if UNITY_EDITOR
        Handles.color = Color.white;
#endif
        // Draw detection sphere
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(origin, viewRadius);

        // Fetch basis vectors
        Vector3 forward = _fovLocation.forward;  // full 3D forward
        Vector3 up = _fovLocation.up;
        Vector3 right = _fovLocation.right;

        // Horizontal bounds: rotate forward around head.up
        Vector3 rightBound = Quaternion.AngleAxis(hAng, up) * forward;
        Vector3 leftBound = Quaternion.AngleAxis(-hAng, up) * forward;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, rightBound * viewRadius);
        Gizmos.DrawRay(origin, leftBound * viewRadius);

        // Vertical bounds: rotate forward around head.right
        Vector3 upperBound = Quaternion.AngleAxis(vAng, right) * forward;
        Vector3 lowerBound = Quaternion.AngleAxis(-vAng, right) * forward;

        Gizmos.DrawRay(origin, upperBound * viewRadius);
        Gizmos.DrawRay(origin, lowerBound * viewRadius);


        /*if (_fovLocation == null) return;

        Vector3 origin = _fovLocation.position;
        float viewRadius = _proximityRadius;

#if UNITY_EDITOR
        Handles.color = Color.white;
#endif
        DebugExtension.DebugWireSphere(origin, Color.white, viewRadius);

        // --- stable basis ---
        Vector3 upAxis = _fovLocation.up.normalized;                                   // vertical follows
        Vector3 fwdYaw = Vector3.ProjectOnPlane(_fovLocation.forward, Vector3.up).normalized; // yaw-only forward
        if (fwdYaw.sqrMagnitude < 1e-6f)
            fwdYaw = new Vector3(_fovLocation.forward.x, 0f, _fovLocation.forward.z).normalized;
        Vector3 rightAxis = Vector3.Cross(upAxis, fwdYaw).normalized;
        Vector3 fwd = Vector3.Cross(rightAxis, upAxis).normalized;                 // orthonormal forward

        // Horizontal FOV (same style, just use yaw-only forward)
        float hAng = _fovViewangle * _horizontalAngleMultiplier;
        Vector3 right = Quaternion.Euler(0f, hAng, 0f) * fwdYaw;
        Vector3 left = Quaternion.Euler(0f, -hAng, 0f) * fwdYaw;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, right * _proximityRadius);
        Gizmos.DrawRay(origin, left * _proximityRadius);

        // Vertical FOV (rotate around the agent's right axis, not world X)
        float vAng = _fovViewangle * _verticalAngleMultiplier;
        Vector3 up = Quaternion.AngleAxis(-vAng, rightAxis) * fwd;
        Vector3 down = Quaternion.AngleAxis(vAng, rightAxis) * fwd;

        Gizmos.DrawRay(origin, up * _proximityRadius);
        Gizmos.DrawRay(origin, down * _proximityRadius);*/

        /*if (_fovLocation == null) return;

      

        Vector3 origin = _fovLocation.position;
        float viewRadius = _proximityRadius;
        //float viewAngle = _fovViewangle;

        // Draw vision radius
#if UNITY_EDITOR
        Handles.color = Color.white;
#endif
        // Handles.DrawWireArc(origin, Vector3.up, Vector3.forward, 360, viewRadius);
        // Handles.DrawWireArc(origin, Vector3.right, Vector3.forward, 360, viewRadius);
        // DebugExtension.DrawCircle(origin, Vector3.up, viewRadius);
        DebugExtension.DebugWireSphere(origin, Color.white, viewRadius);

        // Horizontal FOV
        Vector3 right = Quaternion.Euler(0, _fovViewangle * _horizontalAngleMultiplier, 0) * _fovLocation.forward;
        Vector3 left = Quaternion.Euler(0, -_fovViewangle * _horizontalAngleMultiplier, 0) * _fovLocation.forward;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_fovLocation.position, right * _proximityRadius);
        Gizmos.DrawRay(_fovLocation.position, left * _proximityRadius);

        // Vertical FOV
        Vector3 up = Quaternion.Euler(-_fovViewangle * _verticalAngleMultiplier, 0, 0) * _fovLocation.forward;
        Vector3 down = Quaternion.Euler(_fovViewangle * _verticalAngleMultiplier, 0, 0) * _fovLocation.forward;

        Gizmos.DrawRay(_fovLocation.position, up * _proximityRadius);
        Gizmos.DrawRay(_fovLocation.position, down * _proximityRadius); */


    }




    #region Redundant Code

    protected void SetDetectionPhaseParams(ref FOVPhaseParams fovParams)
    {
        //fovParams.traceComponent = _aiTraceComponent;
        //fovParams.detectionOrigin = _fovLocation;
        //fovParams.detectionResults = _detectionPhaseResults;
        //fovParams.detectionRadius = _detectionPhaseRadius;
        //fovParams.detectionTargetMask = _fovTargetMask;

    }

    protected void SetEvaluationPhaseParams(ref FOVPhaseParams fovParams, Collider targetCollider)
    {
        /*Vector3 waistPos = transform.position + Vector3.up * _waistHeight;
        Vector3 eyePos = transform.position + Vector3.up * _eyeHeight;
        Vector3 sweepCenter = (waistPos + eyePos) * 0.5f;
        Vector3 directionTotarget = TargetingUtility.GetDirectionToTarget(targetCollider.bounds.center, sweepCenter);
        fovParams.evaluationStart = waistPos;
        fovParams.evaluationEnd = eyePos;
        fovParams.evaluationRadius = 0.4f;
        fovParams.evaluationDirection = directionTotarget;
        fovParams.evaluationDistance = _detectionPhaseRadius;
        fovParams.evaluationHitPoints = _evaluationPhaseResults;
        fovParams.targetCollider = targetCollider;
        fovParams.evaluationTargetPosition = targetCollider.bounds.center;
        fovParams.horizontalViewAngle = _horizontalTargetViewAngle * 0.5f;
        fovParams.verticalViewAngle = _verticalTargetViewAngle * 0.75f;*/
    }

    protected void SetTargetingPhaseParams(ref FOVPhaseParams fovParams)
    {
        fovParams.targetingBlockingMask = _fovBlockingMask;
        fovParams.ownerOrigin = transform;
        fovParams.shootOrigin = _bulletSpawnLocation;
    }


    protected override void OutOfAmmo()
    {
        _enemyEventManager.AnimationTriggered(AnimationAction.Reload);
    }
   /* protected void UpdateFOVResults(bool targetSeen)
    {
        _enemyEventManager.TargetSeen(targetSeen);

        if (!targetSeen)
        {
            _enemyEventManager.FacingTarget(false);
            // SetFacingtarget(false);
        }
    }*/

  /*  protected override void ReloadingGun(bool isReloading)
    {
        _isReloading = isReloading;
        if (_isReloading)
        {
            //_weapon.Reload();
        }
    }*/

    protected void SetAimReady(bool isReady)
    {
        _isAimReady = isReady;
    }

    /* protected void OldFOV()
     {
         _fovPhaseParams = new FOVPhaseParams();
         SetDetectionPhaseParams(ref _fovPhaseParams);
         bool targetSeen = false;
         int detectedCount = this.RunDetectionPhase(_fovPhaseParams);

         if (detectedCount == 0)
         {
             UpdateFOVResults(targetSeen);
             return;
         }

         for (int i = 0; i < detectedCount; i++)
         {
             int hitCount;
             SetEvaluationPhaseParams(ref _fovPhaseParams, _detectionPhaseResults[i]);
             this.RunEvaluationPhase(_fovPhaseParams, out hitCount);

             if (hitCount == 0) { continue; }

             SetTargetingPhaseParams(ref _fovPhaseParams);

             if (this.RunTargetingPhase(_fovPhaseParams, hitCount))
             {
                 UpdateFOVResults(true);
                 bool facingTarget = this.TargetWithinShootingRange(_aiTraceComponent, _fovLocation, _detectionPhaseResults[i].ClosestPointOnBounds(_fovLocation.position), _shootAngleThreshold * 0.5f, _shootAngleThreshold * 1.25f);
                 SetFacingtarget(facingTarget);

                 return;
             }

         }

     }*/

    protected bool _isAimReady = false;
    protected bool _isfacingTarget = false;

    protected void SetFacingtarget(bool isFacingTarget)
    {
        _isfacingTarget = isFacingTarget;
    }


    protected override FireConditions GetFireState()
    {
       // if (_targetDead) return FireConditions.TargetDied;
      //  if (!_targetInView) return FireConditions.TargetNotInView;
        if (_meleeTriggered) return FireConditions.Meleeing;
        if (!_isAimReady || !_isfacingTarget) return FireConditions.NotAiming;
        return base.GetFireState();
    }
    #endregion
}
