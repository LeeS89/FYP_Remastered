using System;
using UnityEngine;

[System.Serializable]
public class AbilityParams
{
    [SerializeField] public AbilityDef _def;


    [SerializeField] private PoolIdSO _startPhasePoolId;
    [SerializeField] private PoolIdSO _impactPhasePoolId;
    [SerializeField] private PoolIdSO _endPhasePoolId;
    private Action<string, IPoolManager> PoolRequestCallback;
    private IPoolManager _startPhasePool;
    private IPoolManager _impactPhasePool;
    private IPoolManager _endPhasePool;

    public void Initialize()
    {
        PoolRequestCallback = OnPoolReceived;
        if (_startPhasePoolId != null) this.RequestPool(_startPhasePoolId.Id, PoolRequestCallback);
        if (_impactPhasePoolId != null) this.RequestPool(_impactPhasePoolId.Id, PoolRequestCallback);
        if (_endPhasePoolId != null) this.RequestPool(_endPhasePoolId.Id, PoolRequestCallback);
    }

    private void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (string.IsNullOrEmpty(poolId) || pool == null) return;

        if (poolId == _startPhasePoolId.Id) _startPhasePool = pool;
        if (poolId == _impactPhasePoolId.Id) _impactPhasePool = pool;
        if (_endPhasePoolId != null && _endPhasePoolId.Id == poolId) _endPhasePool = pool;
    }

    public string AbilityId() => _def.Tag.Id;

    public IPoolManager GetPoolRef(CuePhase phase)
    {
        return phase switch
        {
            CuePhase.Start => _startPhasePool,
            CuePhase.Impact => _impactPhasePool,
            CuePhase.End => _endPhasePool,
            _ => null
        };
    }


}
