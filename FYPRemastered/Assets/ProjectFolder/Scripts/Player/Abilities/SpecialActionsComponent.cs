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
        _enemyEventManager.OnShoot += ShootGun;
        _enemyEventManager.OnMelee += SetMeleeTriggered;
        _enemyEventManager.OnAimingLayerReady += SetAimReady;
        _enemyEventManager.OnPlayerSeen += UpdateTargetVisibility;
        _enemyEventManager.OnFacingTarget += SetFacingtarget;
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _enemyEventManager.OnShoot -= ShootGun;
        _enemyEventManager.OnMelee -= SetMeleeTriggered;
        _enemyEventManager.OnAimingLayerReady -= SetAimReady;
        _enemyEventManager.OnPlayerSeen -= UpdateTargetVisibility;
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

    private void LateUpdate()
    {
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
        if (_targetDead) return FireConditions.TargetDied;
        if (_meleeTriggered) return FireConditions.Meleeing;
        if(!_isAimReady || !_isfacingTarget) return FireConditions.NotAiming;
        return base.GetFireState();
    }

    
}
