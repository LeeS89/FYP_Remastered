
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//using RangeAttribute = UnityEngine.RangeAttribute;

public class CombatComponentMigration : BaseAbilitiesMigration
{
    protected EnemyEventManager _enemyEventManager;

    [Header("Field of view Origin")]
    [SerializeField] public Transform _fovLocation;
   /* [Header("Max Field of view targets")]
    [SerializeField] private int _maxFovTraceResults = 5;*/
    [Header("Field of view proximity phase radius")]
    [SerializeField] public float _proximityRadius = 5f; // Make Protected later

   /* [Header("Field of view evaluation phase")]
    [Header("start and end points + radius of capsule cast in FOV evaluation phase")]
    [SerializeField] private float _waistHeight = 1.0f;
    [SerializeField] private float _eyeHeight = 1.8f;
    [SerializeField] private float _fovEvaluationRadius = 0.4f;*/

    [Header("Field of view angle with horizontal & vertical multipliers")]
    [Range(0, 360)] public float _fovViewangle;
    [Range(0, 2)] public float _horizontalAngleMultiplier;
    [Range(0, 2)] public float _verticalAngleMultiplier;


   /* [Header("Field of view region - Melee target, FOV obstruction, and FOV target masks")]
    [SerializeField] private LayerMask _meleeCheckMask;
    [SerializeField] private LayerMask _fovTargetMask;
    [SerializeField] private LayerMask _fovBlockingMask;*/

    [Header("Melee attack check interval")]
    [SerializeField] private float _meleeCheckInterval = 0.2f;

    [Header("Melee trigger radius")]
   // [SerializeField] private float _meleeCheckRadius = 1.5f;
    protected bool _meleeTriggered = false;
    private Coroutine _meleeCheckCoroutine;
    private WaitForSeconds _meleeCheckWait;
    private bool _evaluatingMeleeCheck = false;

    public FieldOfViewParamsObsolete _fovParams;

    [Range(0, 360)] public float _shootAngleThreshold;


    private AITraceComponent _aiTraceComponent;


    [SerializeField] private Collider[] _meleeResults;

    private Action<bool, bool> _fovCallback;


    public NPCWeaponManager _weaponManager;
    private FieldOfViewManagerObsolete _fovhandler;

    [SerializeField] private List<EquippableBase> _availableWeapons;
    private Dictionary<WeaponType, IEquippable> _weaponStore = new(5);
   // private FieldOfViewParamsObsolete _fovParams;


    // Possible updates to shooting condition of within aiming angle
    // 

    /// <summary> Needed in New Setup
    /// Events to update the Weapon handler - Switch weapon (Pass in enum)
    /// Simple indicator of wether the weapon can continue updating
    /// Conditions for firing will be Contained to this class
    /// Weapon handler will manage itself once this component indicates firing can occur
    /// Conditions to allow weapon to update - not dead, target seen, within aiming angle, aim ready
    /// Not meleeing
    /// </summary>
    /// <param name="isDead"></param>











    //   private AgentWeaponHandlerObsolete _weaponHandler;
    //[SerializeField] protected bool _targetSeen = false;



    protected override void DeathStatusUpdated(bool isDead)
    {
        base.DeathStatusUpdated(isDead);

        if (IsOwnerDead)
        {
            OnFieldOfViewComplete(false, false);
         
        }
        else
        {
           
        }
    }

    public bool IsOwnerDead { get; protected set; }

   

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
        _fovhandler = new FieldOfViewManagerObsolete(_enemyEventManager, _fovParams, _aiTraceComponent);

        _enemyEventManager.OnFieldOfViewCallback += OnFieldOfViewComplete;


        // _enemyEventManager.OnMeleeAttackPerformed += EvaluateMeleeAttackResults;
        
        _enemyEventManager.OnMelee += SetMeleeTriggered;

        Transform player = GameManager.Instance.GetPlayerPosition(PlayerPart.DefenceCollider);

       

        RegisterGlobalEvents();

        foreach(var eq in _availableWeapons)
        {
            WeaponType type = eq.EquippableType;
            if(!_weaponStore.TryAdd(type, eq))
            {
                Debug.LogError("Cannot Add duplicate weapons");
            }
        }

    }



    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _meleeCheckWait = null;

        _meleeResults = null;
       // _enemyEventManager.OnMeleeAttackPerformed -= EvaluateMeleeAttackResults;

        _enemyEventManager.OnMelee -= SetMeleeTriggered;

        _enemyEventManager.OnFieldOfViewCallback -= OnFieldOfViewComplete;

        base.UnRegisterLocalEvents(_enemyEventManager);
        UnRegisterGlobalEvents();

    }

    protected override void RegisterGlobalEvents()
    {
        base.RegisterGlobalEvents();
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;


    }
    protected override void UnRegisterGlobalEvents()
    {
        base.UnRegisterGlobalEvents();
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;

    }


    public Transform _bulletSpawnPoint;
    private void InitializeFOVParams()
    {
       /* _fovParams = new FieldOfViewParamsObsolete
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

        );*/
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
       // UpdateWeaponHandler();

    }

    private void UpdateWeaponHandler()
    {
      /*  if (_weaponhandler == null *//*|| !_canupdateweapon*//*) { return; }

        _weaponhandler.updateequippedweapon();*/
    }

    private bool _updateFOV = false;

    private void FieldOfViewEvaluation()
    {
        if (!_updateFOV || _fovhandler == null) { return; }

        _fovhandler?.Tick();


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
               // _meleeCheckCoroutine = StartCoroutine(MeleeCheckRoutine());
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

   /* private IEnumerator MeleeCheckRoutine()
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
    }*/

    private void PerformMeleeAttack()
    {
        _enemyEventManager.PursuitConditionChanged(false);
        _enemyEventManager.TriggerAnimation(AnimationCue.Melee);
    }

  /*  private void EvaluateMeleeAttackResults()
    {
        int targets = _aiTraceComponent.CheckTargetWithinCombatRange(_fovLocation.position, _meleeResults, _meleeCheckRadius, _meleeCheckMask);

        if (targets == 0) { return; }

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
    }*/




    protected override void OnSceneStarted()
    {
        base.OnSceneStarted();
        _canUpdateWeapon = true;
        _updateFOV = true;
        EquipResult result = _weaponManager.EquipWeapon(_weaponStore[WeaponType.Poison]);
        Debug.LogError("Weapon equip result: "+result.ToString());
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

        if(seen) _weaponManager.TryUseWeapon();
         else _weaponManager.CancelWeaponUse();
        //SetFacingtarget(inShootingAngle);
    }

    protected void CheckFieldOfView()
    {
        if (_fovhandler == null || _aiTraceComponent == null) { return; }
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


      


    }




    #region Redundant Code

    

    protected void SetTargetingPhaseParams(ref FOVPhaseParams fovParams)
    {
      //  fovParams.targetingBlockingMask = _fovBlockingMask;
        fovParams.ownerOrigin = transform;
        fovParams.shootOrigin = _bulletSpawnLocation;
    }


    protected override void OutOfAmmo()
    {
        _enemyEventManager.TriggerAnimation(AnimationCue.Reload);
    }
   

    protected void SetAimReady(bool isReady)
    {
        _isAimReady = isReady;
    }

   
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
