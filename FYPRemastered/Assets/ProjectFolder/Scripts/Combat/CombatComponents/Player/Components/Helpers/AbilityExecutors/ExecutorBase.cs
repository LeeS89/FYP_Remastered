using System;
using UnityEngine;

public abstract class ExecutorBase : IEffectExecutor
{
    public string Id { get; private set; }

    public bool IsReady { get; protected set; } = true;

    protected Action spendResourcesNotification;

    public ExecutorBase(Action spendCallback) => spendResourcesNotification = spendCallback;


    public abstract void Execute(in AbilityContext context, EffectDef def, CuePhase phase, IPoolManager pool = null);

 
    

   // public virtual void UpdatePool(PoolIdSO poolId, Action<bool> actionCompleteCallback = null) { }

   // protected virtual void OnPoolReceived(string poolId, IPoolManager pool) { }
    
}
