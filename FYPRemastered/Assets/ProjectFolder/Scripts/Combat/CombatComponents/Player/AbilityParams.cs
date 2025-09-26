using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityParams
{
    [SerializeField] public AbilityDef _def;


    [SerializeField] private PoolIdSO _startPhasePoolId;
    [SerializeField] private PoolIdSO _impactPhasePoolId;
    [SerializeField] private PoolIdSO _endPhasePoolId;

    [SerializeField] private List<PoolIdSO> _availablePools;
    private Dictionary<string, IPoolManager> _pools = new(5);

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

        foreach(var pId in _availablePools)
        {
            string id = pId.Id;
            this.RequestPool(id, PoolRequestCallback);
        }
    }

    private void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (string.IsNullOrEmpty(poolId) || pool == null) return;

        if (poolId == _startPhasePoolId.Id) _startPhasePool = pool;
        if (poolId == _impactPhasePoolId.Id) _impactPhasePool = pool;
        if (_endPhasePoolId != null && _endPhasePoolId.Id == poolId) _endPhasePool = pool;

        _pools[poolId] = pool;
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

    public void ChangePoolRef(CuePhase phase, string id)
    {
        if (_pools.TryGetValue(id, out var pool))
        {
            SetPhasePool(phase, pool);
        }
    }

    private void SetPhasePool(CuePhase phase, IPoolManager pool)
    {
        switch (phase)
        {
            case CuePhase.Start:
                _startPhasePool = pool;
                break;
            case CuePhase.Impact:
                _impactPhasePool = pool;
                break;
            case CuePhase.End:
                _endPhasePool = pool;
                break;
            default:
                return;
        }
    }
}
