using UnityEngine;



[RequireComponent(typeof(ProjectileEventManager))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ProjectileCollisionComponent))]

public abstract class Projectile : ComponentEvents, IPoolable
{
    [Header("Owning Pool - Set by the pool itself")]
    protected IPoolManager _projectilePool;
    
  
    [Header("Time to disable")]
    [SerializeField] protected float _lifespan = 5f;
    protected float _timeOut;

    [Header("Freezable Projectiles retrieve this value from a job that culls when too close, to prevent overdraw")]
    [SerializeField] protected float _distanceToPlayer;
   
    
    [Header("Components")]
    protected ProjectileEventManager _projectileEventManager;
    protected ProjectileMovementHandler _movementHandler;

    [Header("The GameObject which activates the projectile")]
    protected GameObject Owner { get; private set; }

    [SerializeField] protected float _projectileSpeed;

    protected byte _state;
    protected const byte IsActive = 1 << 0;
    public static readonly byte IsFrozen = 1 << 1;
    protected const byte IsCulled = 1 << 2;

  
    protected void SetState(byte state) => _state |= state;
    protected void ClearState(byte state) => _state &= (byte)~state;
    public bool HasState(byte state) => (_state & state) != 0;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _projectileEventManager = eventManager as ProjectileEventManager;
        ComponentRegistry.Register<IPoolable>(gameObject, this);
        EnsureCollider();
        AttachMovementHandler();
        //_projectileEventManager.OnReverseDirection += UnFreeze; => Changing to Deflect
        _projectileEventManager.OnExpired += OnExpired;
       

        _timeOut = _lifespan;
       
    }

    protected virtual void AttachMovementHandler() => _movementHandler = new ProjectileMovementHandler(_projectileEventManager, GetComponent<Rigidbody>());
    

    private void EnsureCollider()
    {

        var existingCol = GetComponentInChildren<Collider>(true);

        if (existingCol != null)
        {
            CreateDefaultColliderIfNoneExists(existingCol.gameObject, exists: true);
            return;
        }
      
        var mr = GetComponentInChildren<MeshRenderer>(true);
        var mf = GetComponentInChildren<MeshFilter>(true);

        var meshObj = mr != null ? mr.gameObject
                    : mf != null ? mf.gameObject
                    : gameObject;


        CreateDefaultColliderIfNoneExists(meshObj, exists : false);
    }

    protected virtual void CreateDefaultColliderIfNoneExists(GameObject target, bool exists)
    {
        if (exists) return;
        target.AddComponent<SphereCollider>();
    }




    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        ComponentRegistry.Unregister<IPoolable>(gameObject);
        _projectileEventManager.OnExpired -= OnExpired;
        _movementHandler.OnInstanceDestroyed();
        _movementHandler = null;
       // _projectileEventManager.OnReverseDirection -= UnFreeze;
        
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _projectileEventManager = null;
    }

  

    protected virtual Vector3 GetDirectionToTarget()
    {
        Vector3 directionTotarget = TargetingUtility.GetDirectionToTarget(Owner, gameObject, true);
        return directionTotarget;
    }

  


    public virtual void LaunchPoolable(GameObject projectileOwner)
    {
        Owner = projectileOwner;

        SetState(IsActive);
        _projectileEventManager.Launched();
        _timeOut = _lifespan;
        
    }

    protected virtual void Update()
    {
        if (!HasState(IsActive)) { return; }

        if (_timeOut > 0f) _timeOut -= Time.deltaTime;
        else OnExpired();

    }

    protected virtual void FixedUpdate() => _movementHandler?.FixedTick();


    protected virtual void OnExpired() => _projectilePool.Release(gameObject);

    // public virtual void Freeze() { }
    // public virtual void UnFreeze() { }

    protected virtual void RemoveFromJob() { } 

    public virtual void SetDistanceToPlayer(float distance) => _distanceToPlayer = distance;

    public void SetParentPool(IPoolManager manager) => _projectilePool = manager;


    protected virtual void OnDisable() => _movementHandler?.OnDisable();


}
