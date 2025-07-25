using System;
using System.Collections;
using UnityEngine;

public class CombatComponent : BaseAbilities
{
    protected EnemyEventManager _enemyEventManager;

    [Header ("Field of view Origin")]
    [SerializeField] public Transform _fovLocation;
    [Header("Max Field of view targets")]
    [SerializeField] private int _maxFovTraceResults = 5;
    [Header("Field of view proximity phase radius")]
    [SerializeField] protected float _proximityRadius = 5f;

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

    protected bool _targetInView = false;
    protected bool _targetDead = false;

    private float _shootInterval = 2f;
    private float _nextShootTime = 0f;

    [Range(0, 360)] public float _shootAngleThreshold;


    private AITraceComponent _aiTraceComponent;
    
  
    [SerializeField] private Collider[] _meleeResults;
  
    
    
    

    private FieldOfViewHandler _fovhandler;
    private FieldOfViewParams _fovParams;
    

    WeaponHandlerBase _weapon;
    //[SerializeField] protected bool _targetSeen = false;

    protected void SetMeleeTriggered(bool isMelee)
    {
        _meleeTriggered = isMelee;
    }

   

    protected void UpdateTargetVisibility(bool targetInView)
    {
        _targetInView = targetInView;
    }

   
  
    protected override void OnPlayerDied()
    {
        _targetDead = true;
    }

    protected override void OnPlayerRespawned()
    {
        _targetDead = false;
    }

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _enemyEventManager = eventManager as EnemyEventManager;
        base.RegisterLocalEvents(_enemyEventManager);

        _aiTraceComponent = new AITraceComponent();
      
        _meleeResults = new Collider[2];
      
        _meleeCheckWait = new WaitForSeconds(_meleeCheckInterval);


        InitializeFOVParams();
        _fovhandler = new FieldOfViewHandler(_aiTraceComponent, _fovParams);

        

        _enemyEventManager.OnMeleeAttackPerformed += EvaluateMeleeAttackResults;
        //_enemyEventManager.OnShoot += ShootGun;
        _enemyEventManager.OnMelee += SetMeleeTriggered;
       // _enemyEventManager.OnAimingLayerReady += SetAimReady;
       // _enemyEventManager.OnTargetSeen += UpdateTargetVisibility;
        //_enemyEventManager.OnFacingTarget += SetFacingtarget;
    }

   

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _meleeCheckWait = null;
       // _detectionPhaseResults = null;
        _meleeResults = null;
        _enemyEventManager.OnMeleeAttackPerformed -= EvaluateMeleeAttackResults;
        //_enemyEventManager.OnShoot -= ShootGun;
        _enemyEventManager.OnMelee -= SetMeleeTriggered;
       // _enemyEventManager.OnAimingLayerReady -= SetAimReady;
        //_enemyEventManager.OnTargetSeen -= UpdateTargetVisibility;
        //_enemyEventManager.OnFacingTarget -= SetFacingtarget;
        base.UnRegisterLocalEvents(_enemyEventManager);
        //_enemyEventManager = null;
    }

    protected override void RegisterGlobalEvents()
    {
        base.RegisterGlobalEvents();
        GameManager.OnPlayerDied += OnPlayerDied;
        GameManager.OnPlayerRespawn += OnPlayerRespawned;   

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
        if (_testMelee)
        {
            _evaluatingMeleeCheck = true;
            ToggleMeleeCheckRoutine(true);
            _testMelee = false;
        }

        if(_weapon == null) { return; }

        if(Time.time >= _nextShootTime)
        {
            CheckFieldOfView();
           /* if (CanShoot())
            {
                _enemyEventManager.AnimationTriggered(AnimationAction.Shoot);
            }*/
           _weapon.TryShootGun();
            _nextShootTime = Time.time + _shootInterval;
        }
    }

    protected override void UnRegisterGlobalEvents()
    {
        base.UnRegisterGlobalEvents();
        GameManager.OnPlayerDied -= OnPlayerDied;
        GameManager.OnPlayerRespawn -= OnPlayerRespawned;
    }


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
        _weapon = new WeaponHandlerBase();

        _weapon.EquipGun(owner, _enemyEventManager , bulletSpawnLocaiton, clipCapacity, target);
        // base.GunSetup(owner, _enemyEventManager, bulletSpawnLocaiton, clipCapacity, target);
    }

   

   

    


    private void OnFieldOfViewComplete(bool seen, bool inShootingAngle)
    {
        //UpdateFOVResults(seen);
        _enemyEventManager.TargetSeen(seen);
        _enemyEventManager.FacingTarget(inShootingAngle);
        //SetFacingtarget(inShootingAngle);
    }

    protected void CheckFieldOfView()
    {
        if(_fovhandler == null || _aiTraceComponent == null) { return; }
        _fovhandler.RunFieldOfViewSweep(OnFieldOfViewComplete, true);
    }


    void OnDrawGizmos()
    {
        if (_fovLocation == null) return;

        Gizmos.color = Color.green;

        // Horizontal FOV
        Vector3 right = Quaternion.Euler(0, _fovViewangle * _horizontalAngleMultiplier, 0) * _fovLocation.forward;
        Vector3 left = Quaternion.Euler(0, -_fovViewangle * _horizontalAngleMultiplier, 0) * _fovLocation.forward;

        Gizmos.DrawRay(_fovLocation.position, right * 5f);
        Gizmos.DrawRay(_fovLocation.position, left * 5f);

        // Vertical FOV
        Vector3 up = Quaternion.Euler(-_fovViewangle * _verticalAngleMultiplier, 0, 0) * _fovLocation.forward;
        Vector3 down = Quaternion.Euler(_fovViewangle * _verticalAngleMultiplier, 0, 0) * _fovLocation.forward;

        Gizmos.DrawRay(_fovLocation.position, up * 5f);
        Gizmos.DrawRay(_fovLocation.position, down * 5f);
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
    protected void UpdateFOVResults(bool targetSeen)
    {
        _enemyEventManager.TargetSeen(targetSeen);

        if (!targetSeen)
        {
            _enemyEventManager.FacingTarget(false);
            // SetFacingtarget(false);
        }
    }

    protected override void ReloadingGun(bool isReloading)
    {
        _isReloading = isReloading;
        if (_isReloading)
        {
            //_weapon.Reload();
        }
    }

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
        if (_targetDead) return FireConditions.TargetDied;
        if (!_targetInView) return FireConditions.TargetNotInView;
        if (_meleeTriggered) return FireConditions.Meleeing;
        if (!_isAimReady || !_isfacingTarget) return FireConditions.NotAiming;
        return base.GetFireState();
    }
    #endregion
}
