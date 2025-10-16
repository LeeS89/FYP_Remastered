using System;
using UnityEngine;

public abstract class VFXBase : ComponentEvents
{
    [Header("ID's of the SFX & VFX Pools to be used on Collision")]
    [SerializeField] protected PoolIdSO CollisionParticlePoolId;
    [Header("ID's of the SFX & VFX Pools to be used on Spawn - Such as a MuzzleFlash")]
    [SerializeField] protected PoolIdSO SpawnParticlePoolId;
    // Spawn Audio pool
    

    [Header("Pool References")]
    public IPoolManager CollisionParticlePoolManager { get; protected set; }
    public IPoolManager SpawnParticlePoolManager { get; protected set; }
   

    [Header("Pooled Object SpawnPoints - Optional, can also be passed via the function call")]
    [SerializeField] protected Transform SpawnParticleLocaton;

    [Header("Called when a request for a pool is completed")]
    protected Action<string, IPoolManager> PoolRequestCallback;

    protected CombatEventManager _cmbtEventManager;


    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _cmbtEventManager = eventManager as CombatEventManager;
        base.RegisterLocalEvents(_cmbtEventManager);
        PoolRequestCallback = OnPoolReceived;
        _cmbtEventManager.OnSpawnParticle += SpawnParticle;
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _cmbtEventManager.OnSpawnParticle -= SpawnParticle;
        base.UnRegisterLocalEvents(eventManager);
    }


    public abstract override void InitialzeLocalPools();

    protected abstract void OnPoolReceived(string poolId, IPoolManager pool);

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        PoolRequestCallback = null;
        _cmbtEventManager = null;
    }

  
  

    protected virtual void FireCollisionParticle(Collision collision)
    {
      
        if (CollisionParticlePoolManager == null) return;
       
        ContactPoint contact = collision.GetContact(0);

        Vector3 pos = contact.point;
        Vector3 normal = contact.normal;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
        SpawnPooledObject(CollisionParticlePoolManager, pos, rotation);
    }

    protected virtual void SpawnParticle(Vector3? position = null, Quaternion? rotation = null)
    {
        if (SpawnParticlePoolManager == null) return;
       
        Vector3 pos;
        if (position != null) pos = position.Value;
        else if (SpawnParticleLocaton != null) pos = SpawnParticleLocaton.position;
        else
        {
#if UNITY_EDITOR
            throw new NullReferenceException("Must provide a valid position to spawn particle");
#else
            return;
#endif
        }
        SpawnPooledObject(SpawnParticlePoolManager, pos, rotation);
    }

    protected virtual void SpawnPooledObject(IPoolManager pm, Vector3 position, Quaternion? rotation = null)
    {
        if (pm == null) return;

        PoolKind Kind = pm.GetPoolType();
        Quaternion rot = rotation ?? Quaternion.identity;
        if (Kind == PoolKind.ParticleSystem)
        {
            var hit = pm.GetFromPool(position, rot) as ParticleSystem;
            hit.Play();
        }
        else if (Kind == PoolKind.GameObject)
        {
            //Quaternion finalRot = rotation != null ? Quaternion.FromToRotation(Vector3.up, rotation.Value) : Quaternion.identity;//normal.Value : Quaternion.identity;
            pm.GetFromPool(position, rot/*Quaternion.FromToRotation(Vector3.up, normal.Value)*/);
        }
        else if( Kind == PoolKind.Audio)
        {
            var sfx = pm.GetFromPool(position, rot) as AudioSource;
            sfx.Play();
        }
    }
}
