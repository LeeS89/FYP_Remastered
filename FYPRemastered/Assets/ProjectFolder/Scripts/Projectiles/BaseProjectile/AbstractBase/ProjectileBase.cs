using UnityEngine;

public enum MovementType
{
    None,
    Acceleration,
    Velocity,
    FixedSpeedKinematic,
    Impulse
}

[RequireComponent(typeof(ProjectileEventManager))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ProjectileCollisionComponent))]
public class ProjectileBase : ComponentEvents, IPoolable
{
    [Header("Owning Pool - Set by the pool itself")]
    protected IPoolManager _projectilePool;
    [SerializeField] protected MovementType _movementType = MovementType.None;

    [Header("Time to disable")]
    [SerializeField] protected float _lifespan = 5f;
    protected float _timeOut;

    

    [Header("Components")]
    protected ProjectileEventManager _projectileEventManager;
    protected ProjectileMovementManagerBase _movementHandler;

    [Header("The GameObject which activates the projectile")]
    protected GameObject Owner { get; private set; }

    [Header("Initial Speed")]
    [SerializeField] protected float _projectileSpeed;

    [Tooltip("Acceleration and maxSpeed only apply to Acceleration MovementType - Automatically sets Rigidbody to non kinematic")]
    [SerializeField] protected float _acceleration = 0f;
    [SerializeField] protected float _maxSpeed = 0f;

    [Tooltip("When Movement type == Impulse, toggles this on automatically, but can be used for any move type")]
    [SerializeField] protected bool _useGravity = false;

    protected byte _state;
    protected const byte IsActive = 1 << 0;


  

    protected void SetState(byte state) => _state |= state;
    protected void ClearState(byte state) => _state &= (byte)~state;
    public bool HasState(byte state) => (_state & state) != 0;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _projectileEventManager = eventManager as ProjectileEventManager;
        ComponentRegistry.Register<IPoolable>(gameObject, this);
        EnsureCollider();
        AttachMovementHandler();
      
        _projectileEventManager.OnExpired += OnExpired;


        _timeOut = _lifespan;

    }

    protected virtual void AttachMovementHandler() 
        => _movementHandler = new ProjectileMovementManagerBase(_projectileEventManager, GetComponent<Rigidbody>(), _projectileSpeed, _movementType, _acceleration, _maxSpeed, _useGravity);


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


        CreateDefaultColliderIfNoneExists(meshObj, exists: false);
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



    public bool _testlaunch = false;

    public virtual void LaunchPoolable(GameObject projectileOwner)
    {
        Owner = projectileOwner;

        SetState(IsActive);
        _projectileEventManager.Launched();
        _timeOut = _lifespan;

    }

    protected virtual void Update()
    {
        if (_testlaunch)
        {
            LaunchPoolable(gameObject);
            _testlaunch = false;
        }
        if (!HasState(IsActive)) { return; }

        if (_timeOut > 0f) _timeOut -= Time.deltaTime;
        else OnExpired();

    }

    protected virtual void FixedUpdate() => _movementHandler?.FixedTick();

    private void OnEnable()
    {
        SetState(IsActive);
    }

    protected virtual void OnExpired()
    {
   
        ClearState(IsActive);
        _projectilePool?.Release(gameObject);
    }


    public void SetParentPool(IPoolManager manager)
    {
        if (manager == null) return;
        _projectilePool = manager;
    }


    protected virtual void OnDisable() => _movementHandler?.OnDisable();
}
