using System;
using UnityEngine;

public abstract class PoolManagerBase : IPoolManager
{
    public virtual Type ItemType => throw new NotImplementedException();

    public abstract UnityEngine.Object GetFromPool(Vector3 position, Quaternion rotation);
    

    public virtual void PreWarmPool(int count) { }

    public abstract void Release(UnityEngine.Object obj);
    
}
