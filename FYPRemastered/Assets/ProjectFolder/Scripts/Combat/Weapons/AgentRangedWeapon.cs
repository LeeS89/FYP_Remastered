using UnityEngine;


public class AgentRangedWeapon : RangedWeaponBase
{
    private EnemyEventManager _eventManager;
    protected float[] _fireRateOffsets;
    protected Transform _target;
    

    public AgentRangedWeapon(Transform bulletSpawnPoint, Transform target, EnemyEventManager eventManager, GameObject gunOwner, int clipCapacity)
    {
        _eventManager = eventManager;
        _bulletSpawnPoint = bulletSpawnPoint;
        _target = target;
        _gunOwner = gunOwner;
        _clipCapacity = clipCapacity;
        _clipCount = _clipCapacity;

        _eventManager.OnShoot += Fire;
        _eventManager.OnAimingLayerReady += AimStateChanged;
        _eventManager.OnFacingTarget += FacingTargetStatusChanged;
        _eventManager.OnMelee += MeleeTriggered;
        _eventManager.OnTargetSeen += TargetInViewStatusChanged;
       
    }

    #region fire Conditions
    public virtual bool IsAimReady { get; protected set; }
    public virtual bool IsTargetInView { get; protected set; } 
    public virtual bool IsFacingTarget { get; protected set; }
    public virtual bool IsTargetDead { get; protected set; }
    public virtual bool IsMeleeTriggered { get; protected set; }  
    public virtual bool IsOwnerDead { get; protected set; }

    protected void TargetDead() => IsTargetDead = true;
    protected void TargetRespawned() => IsTargetDead = false;
   
    protected void MeleeTriggered(bool isMelee) => IsMeleeTriggered = isMelee;
    protected void OwnerDied(bool ownerDead) => IsOwnerDead = ownerDead;

    protected void FacingTargetStatusChanged(bool isFacing) => IsFacingTarget = isFacing;
  
    protected void TargetInViewStatusChanged(bool inView) => IsTargetInView = inView;

    protected void AimStateChanged(bool isReady) => IsAimReady = isReady;
    #endregion

    public override void Fire()
    {
        if (_clipCount > 0)
        {

            _clipCount--;
            Vector3 _directionToTarget = TargetingUtility.GetDirectionToTarget(_target, _bulletSpawnPoint, true);
            Quaternion bulletRotation = Quaternion.LookRotation(_directionToTarget);

            GameObject obj = _bulletPoolManager.GetFromPool(_bulletSpawnPoint.position, bulletRotation);

            BulletBase bullet = obj.GetComponentInChildren<BulletBase>();
            
            bullet.InitializeBullet(_gunOwner);
        }

        if (_clipCount == 0)
        {
            OutOfAmmo();
        }
    }

    protected override void ReloadStateChanged(bool isReloading)
    {
        base.ReloadStateChanged(isReloading);
        if (isReloading)
        {
            Reload();
        }
    }


    public override FireConditions GetFireState()
    {
        if (IsOwnerDead) return FireConditions.OwnerDied;
        if (IsTargetDead) return FireConditions.TargetDied;
        if (!IsTargetInView) return FireConditions.TargetNotInView;
        if (IsMeleeTriggered) return FireConditions.Meleeing;
        if (!IsAimReady || !IsFacingTarget) return FireConditions.NotAiming;
        return base.GetFireState();
    }

    public override void Reload()
    {
        _clipCount = _clipCapacity;
    }

   

    public override void UpdateWeapon()
    {
        
    }

    protected virtual void OutOfAmmo()
    {
        _eventManager.AnimationTriggered(AnimationAction.Reload);
    }

    public override void OnInstanceDestroyed()
    {
        _eventManager.OnShoot -= Fire;
        _eventManager.OnAimingLayerReady -= AimStateChanged;
        _eventManager.OnFacingTarget -= FacingTargetStatusChanged;
        _eventManager.OnMelee -= MeleeTriggered;
        _eventManager.OnTargetSeen -= TargetInViewStatusChanged;
        _eventManager = null;
        _target = null;
        base.OnInstanceDestroyed();
    }
}
