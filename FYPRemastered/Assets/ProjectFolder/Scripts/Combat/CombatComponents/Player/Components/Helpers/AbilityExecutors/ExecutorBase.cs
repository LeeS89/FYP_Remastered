using System;
using UnityEngine;

public abstract class ExecutorBase : IEffectExecutor
{
    public string Id { get; private set; }

    public bool IsReady { get; protected set; } = true;



    /* public ExecutorBase(string id, Action<string, bool> callback = null)
     {
         Id = id;

     }*/

    public abstract void Execute(in AbilityContext context, EffectDef def, CuePhase phase);

    public virtual void UpdatePool(PoolIdSO poolId, Action<bool> actionCompleteCallback = null) { }

    protected virtual void OnPoolReceived(string poolId, IPoolManager pool) { }
    
}
