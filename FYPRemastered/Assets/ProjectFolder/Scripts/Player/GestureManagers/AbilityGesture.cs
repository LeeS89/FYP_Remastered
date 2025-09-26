using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityGesture : BaseGesture
{
    [SerializeField] private Transform _handAnchor;
    [SerializeField] private AbilityTags _abilityId;
    [SerializeField] private PoolIdSO _poolId;
    private Action<string, IPoolManager> poolCallback;
    private IPoolManager _poolManager;
    private PlayerEventManager _pEventManager;
    private Dictionary<CuePhase, IPoolManager> _abilityPools = new(2);


    [SerializeField] private List<AbilityParams> _abilityParams;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        if (eventManager == null) return;

        _pEventManager = eventManager as PlayerEventManager;
        base.RegisterLocalEvents(_pEventManager);

        if (_poolId == null) return;
        poolCallback = OnPoolReceived;
        this.RequestPool(_poolId.Id, poolCallback);
       
    }

    public override void OnGestureRecognized()
    {
        if (OwnerIsDead) return;
        if (_pEventManager == null || _poolManager == null || _abilityPools == null) return;
        // var ability = AbilityResources.SetAbilityPools(_abilityId, now: Time.time, _abilityPools);//AbilityResources.SetImpactPhasePool(_abilityId, now: Time.time, _poolManager);
        _pEventManager.TryUseAbility(_abilityId, _handAnchor);
    }

    public override void OnGestureReleased()
    {
        if (_pEventManager == null) return;
        _pEventManager.EndAbility(_abilityId);
    }

    private void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (poolId == _poolId.Id) _poolManager = pool;
        _abilityPools[CuePhase.Start] = _poolManager;
        _abilityPools[CuePhase.Impact] = _poolManager;
    }

    protected override void DeathStatusUpdated(bool isDead)
    {
        base.DeathStatusUpdated(isDead);

        if (isDead) OnGestureReleased();
    }

}
