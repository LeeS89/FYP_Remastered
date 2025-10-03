using System;
using UnityEngine;

public class RangedWeapon : WeaponBaseObsolete
{
    public AbilityDef _def;
    public PoolIdSO _poolId;
    private IPoolManager _pool;
    private Action<string, IPoolManager> OnPoolReceived;

    

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        OnPoolReceived = OnPoolRequestComplete;
        if (string.IsNullOrEmpty(_poolId.Id)) this.RequestPool(_poolId.Id, OnPoolReceived);
    }


    private void OnPoolRequestComplete(string id, IPoolManager pool)
    {
        if (string.IsNullOrEmpty(id)) return;

        if (id == _poolId.Id) _pool = pool;
    }

    public AbilityDef GetDef() => _def;

    public override void OnEquipped(AbilityRuntime runtime)
    {
        if (runtime == null || _pool == null) return;
        runtime.ChangePools(CuePhase.Start, _pool);
        runtime.ChangePools(CuePhase.Impact, _pool);
    }

    private void OnDestroy()
    {
        _pool = null;
        OnPoolReceived = null;
    }
}
