using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityGesture : BaseGesture
{
    [SerializeField] private AbilityOrigins _origins;
    [SerializeField] private Transform _handAnchor;
    [SerializeField] private AbilityTags _abilityId;
    [SerializeField] private PoolIdSO _poolId;
    private Action<string, IPoolManager> poolCallback;
    private IPoolManager _poolManager;
    private PlayerEventManager _pEventManager;
    private Dictionary<CuePhase, IPoolManager> _abilityPools = new(2);



    


    /* public override void RegisterLocalEvents(EventManager eventManager)
     {
         if (eventManager == null) return;

         _pEventManager = eventManager as PlayerEventManager;
         base.RegisterLocalEvents(_pEventManager);

         if (_poolId == null) return;
         poolCallback = OnPoolReceived;
         this.RequestPool(_poolId, poolCallback);

     }*/

    public override void Init(IPlaceholderService services, PlayerEventManager manager)
    {
        _pEventManager = manager;

        if (_poolId == null) return;
        poolCallback = OnPoolReceived;
        this.RequestPool(_poolId, poolCallback);
    }

    /// Had to remove IsDead variable
    /// Add new public function to BaseGesture to check if owner is dead
    /// or include toggle that the owner can activate to indicate death state
    public override void OnGestureRecognized()
    {
        //if (IsDead/*OwnerIsDead*/) return;
        if (_pEventManager == null || _poolManager == null || _abilityPools == null) return;
        // var ability = AbilityResources.SetAbilityPools(_abilityId, now: Time.time, _abilityPools);//AbilityResources.SetImpactPhasePool(_abilityId, now: Time.time, _poolManager);
        _pEventManager.TryUseAbility(_abilityId, _origins);
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

    // Need to fix and update to new system
    protected /*override*/ void DeathStatusUpdated(bool isDead)
    {
      //  base.DeathStatusUpdated(isDead);

        if (isDead) OnGestureReleased();
    }

    

    public override void Unload()
    {
        throw new NotImplementedException();
    }

    protected override void Update()
    {
        //throw new NotImplementedException();
    }
}
