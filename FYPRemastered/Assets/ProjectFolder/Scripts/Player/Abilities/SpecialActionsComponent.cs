using System;
using System.Collections;
using UnityEngine;

public class SpecialActionsComponent : BaseAbilities
{
    protected EnemyEventManager _enemyEventManager;

    protected bool _meleeTriggered = false;
    protected bool _isAimReady = false;
    protected bool _isfacingTarget = false;
    protected bool _targetInView = false;
    protected bool _targetDead = false;

    private float _shootInterval = 2f;
    private float _nextShootTime = 0f;

    private Coroutine _meleeCheckCoroutine;
    private WaitForSeconds _meleeCheckWait;
    private bool _evaluatingMeleeCheck = false;
    [SerializeField] private float _meleeCheckInterval = 0.2f;
    public float _meleeCheckRadius = 1.5f;
    private AITraceComponent _aiTraceComponent;
    [SerializeField] public Transform _fovLocation;
    [SerializeField] private Collider[] _fovTraceResults;
    [SerializeField] private Collider[] _meleeResults;
    public LayerMask _meleeCheckMask;

    protected void SetMeleeTriggered(bool isMelee)
    {
        _meleeTriggered = isMelee;
    }

    protected void SetAimReady(bool isReady)
    {
        _isAimReady = isReady;
    }

    protected void UpdateTargetVisibility(bool targetInView)
    {
        _targetInView = targetInView;
    }

    protected void SetFacingtarget(bool isFacingTarget)
    {
        _isfacingTarget = isFacingTarget;
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
        _fovTraceResults = new Collider[2];
        _meleeResults = new Collider[2];
        _meleeCheckWait = new WaitForSeconds(_meleeCheckInterval);

        _enemyEventManager.OnMeleeAttackPerformed += EvaluateMeleeAttackResults;
        _enemyEventManager.OnShoot += ShootGun;
        _enemyEventManager.OnMelee += SetMeleeTriggered;
        _enemyEventManager.OnAimingLayerReady += SetAimReady;
        _enemyEventManager.OnTargetSeen += UpdateTargetVisibility;
        _enemyEventManager.OnFacingTarget += SetFacingtarget;
    }

   

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _meleeCheckWait = null;
        _fovTraceResults = null;
        _meleeResults = null;
        _enemyEventManager.OnMeleeAttackPerformed -= EvaluateMeleeAttackResults;
        _enemyEventManager.OnShoot -= ShootGun;
        _enemyEventManager.OnMelee -= SetMeleeTriggered;
        _enemyEventManager.OnAimingLayerReady -= SetAimReady;
        _enemyEventManager.OnTargetSeen -= UpdateTargetVisibility;
        _enemyEventManager.OnFacingTarget -= SetFacingtarget;
        base.UnRegisterLocalEvents(_enemyEventManager);
        //_enemyEventManager = null;
    }

    protected override void RegisterGlobalEvents()
    {
        base.RegisterGlobalEvents();
        GameManager.OnPlayerDied += OnPlayerDied;
        GameManager.OnPlayerRespawn += OnPlayerRespawned;   

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

        if(_gun == null) { return; }

        if(Time.time >= _nextShootTime)
        {
            if (CanShoot())
            {
                _enemyEventManager.AnimationTriggered(AnimationAction.Shoot);
            }
            _nextShootTime = Time.time + _shootInterval;
        }
    }

    protected override void UnRegisterGlobalEvents()
    {
        base.UnRegisterGlobalEvents();
        GameManager.OnPlayerDied -= OnPlayerDied;
        GameManager.OnPlayerRespawn -= OnPlayerRespawned;
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
                damageable.Knockback(500, transform.forward, 7f, 0.3f);
                break;
            }
        }

        Array.Clear(_meleeResults, 0, _meleeResults.Length);
    }

    public override void GunSetup(GameObject owner, EventManager eventManager, Transform bulletSpawnLocaiton, int clipCapacity, Transform target)
    {
        base.GunSetup(owner, _enemyEventManager, bulletSpawnLocaiton, clipCapacity, target);
    }

    protected override void OutOfAmmo()
    {
        _enemyEventManager.AnimationTriggered(AnimationAction.Reload);
    }

    protected override FireConditions GetFireState()
    {
        Debug.LogError("Calling overridden GetFirestate");
        if (_targetDead) return FireConditions.TargetDied;
        if (_meleeTriggered) return FireConditions.Meleeing;
        if(!_isAimReady || !_isfacingTarget) return FireConditions.NotAiming;
        return base.GetFireState();
    }

    
}
