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

    [Header("The GameObject which activates the projectile")]
    protected GameObject Owner { get; private set; }
   
  

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
        //_projectileEventManager.OnReverseDirection += UnFreeze; => Changing to Deflect
        _projectileEventManager.OnExpired += OnExpired;
       

        _timeOut = _lifespan;
       
    }

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

  


    public virtual void InitializePoolable(GameObject projectileOwner)
    {
        Owner = projectileOwner;

        SetState(IsActive);
        _projectileEventManager.Fired();
        _timeOut = _lifespan;
        
    }

    protected virtual void Update()
    {
        if (!HasState(IsActive)) { return; }

        if (_timeOut > 0f)
        {
            _timeOut -= Time.deltaTime;
        }
        else
        {
            OnExpired();
        }

        

    }

    protected abstract void OnExpired();

   // public virtual void Freeze() { }
   // public virtual void UnFreeze() { }

    protected virtual void RemoveFromJob() { } 

    public virtual void SetDistanceToPlayer(float distance) => _distanceToPlayer = distance;






    public void SetParentPool(IPoolManager manager) => _projectilePool = manager;





    #region redundant
    /*protected virtual void CheckForOverlap()
    {

        Vector3 center = _capsuleCollider.transform.TransformPoint(_capsuleCollider.center); // Convert to world position
        float worldHeight = _capsuleCollider.height * _capsuleCollider.transform.lossyScale.y;
        Vector3 start = center - _capsuleCollider.transform.up * (worldHeight * 0.8f);
        Vector3 end = center + _capsuleCollider.transform.up * (worldHeight * 0.8f);
        float capsuleRadius = _capsuleCollider.radius * Mathf.Max(_capsuleCollider.transform.lossyScale.x, _capsuleCollider.transform.lossyScale.y, _capsuleCollider.transform.lossyScale.z);
        int hits = Physics.OverlapCapsuleNonAlloc(start, end, capsuleRadius * 1.65f, _overlapResults, _layerMask);
        //DebugExtension.DebugCapsule(start, end, Color.blue, capsuleRadius * 1.4f);
        for (int i = 0; i < hits; ++i)
        {
            if (_overlapResults[i].gameObject == _capsuleCollider.gameObject)
            {
                continue;
            }
            //Cull(true);
            break;
        }

    }*/

    #endregion

}
