using System;
using UnityEngine;

public class AbilityBase
{
    protected EventManager _eventManager;
    protected AbilityDefinition _abilityDef;
    protected PoolIdSO BeginPoolId, InProgressPoolId, EndPoolId;
    protected IPoolManager BeginPool, InProgressPool, EndPool;
    protected Action<string, IPoolManager> PoolReceivedCallback;
    protected float _nextTick;
    protected bool _channeling;
    public Transform ExecuteOrigin { get; protected set; } = null;
    public Transform ExecuteDirectionOrigin { get; protected set; } = null;

    public float DirectionOffset { get; protected set; } = 0f;

    public AbilityBase(AbilityDefinition def/*, EventManager eventManager*/)
    {
        _abilityDef = def;
       // _eventManager = eventManager;
    }

    public bool InProgress { get; protected set; } = false;

    protected virtual void OnPoolReceived(string poolId, IPoolManager pool) { }

    protected virtual void InitializePools()
    {
        PoolReceivedCallback = OnPoolReceived;
        if (BeginPoolId != null) this.RequestPool(BeginPoolId.Id, PoolReceivedCallback);
        if (InProgressPoolId != null) this.RequestPool(InProgressPoolId.Id, PoolReceivedCallback);
        if (EndPoolId != null) this.RequestPool(EndPoolId.Id, PoolReceivedCallback);
    }

    public virtual void Tick(float now) { }
    public virtual void FixedTick() { }

    protected virtual void OnInterrupted() { }

    public virtual StatType GetStatParams() => StatType.None;

    protected virtual bool CanActivate() => true;

    public virtual void SetFireRate(FireRate rate) { }

    public virtual bool TryActivate(float now, AbilityOrigins origin = null) => true;

    public virtual void End() { }

    protected virtual int SweepForTargets() { return 0; }

    protected virtual void ImplementPhase(CuePhase phase, Transform origin, Vector3? direction = null) { }

    protected virtual bool HasSufficientResources() => true;

    protected virtual void SpendResources() { }

    protected virtual void Execute(CuePhase phase, Transform origin, Vector3? direction = null, IPoolManager pool = null) { }


}
