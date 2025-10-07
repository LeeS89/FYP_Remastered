using System;
using UnityEngine;

[System.Serializable]
public class AbilityParam
{
    public AbilityTags AbilityIdTag;

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
        if (_startPhasePoolId != null) this.RequestPool(_startPhasePoolId, PoolRequestCallback);
        if (_impactPhasePoolId != null) this.RequestPool(_impactPhasePoolId, PoolRequestCallback);
        if (_endPhasePoolId != null) this.RequestPool(_endPhasePoolId, PoolRequestCallback);
    }

    private void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (string.IsNullOrEmpty(poolId) || pool == null) return;

        if (_startPhasePoolId && _startPhasePoolId.Id == poolId) _startPhasePool = pool;
        if (_impactPhasePoolId && _impactPhasePoolId.Id == poolId) _impactPhasePool = pool;
        if (_endPhasePoolId && _endPhasePoolId.Id == poolId) _endPhasePool = pool;
    }

    public IPoolManager GetPhasePoolRef(CuePhase phase)
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
