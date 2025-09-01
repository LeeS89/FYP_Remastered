using System;
using UnityEngine;

public readonly struct PoolRequest<TPool> where TPool : IPoolManager
{
    public readonly PoolResourceType PoolType;
    public readonly Action<PoolResourceType, TPool> Callback;

    public PoolRequest(PoolResourceType type, Action<PoolResourceType, TPool> cb)
    {
        PoolType = type;
        Callback = cb;
    }
}


public readonly struct FlankPointTargetAndBlockingMasks
{
    public readonly Action<LayerMask, LayerMask, LayerMask> Callback;

    public FlankPointTargetAndBlockingMasks(Action<LayerMask, LayerMask, LayerMask> cb) => Callback = cb;

}
