using System;
using UnityEngine;

public class ProjectileFireExecutor : ExecutorBase
{
    private IPoolManager _pool;
    private EffectDef _def;
    private string _poolId;
  
    private Action<string, IPoolManager> poolCallback;
  

 

    public ProjectileFireExecutor(EffectDef def = null)
    {
       
        if (def == null || def.PoolId == null) return;
        _def = def;
        poolCallback = OnPoolReceived;
        _poolId = _def.PoolId.Id;
        this.RequestPool(_def.PoolId.Id, poolCallback);
    }

    public override void Execute(in AbilityContext context, EffectDef def, CuePhase phase)
    {

        if (def.Kind != EffectKind.SpawnAndFire) return;
        if (_pool == null) return;
       
        // Vector3 direction = context.AbilitydirectionOrigin.forward;
        Quaternion rotation = context.AbilityDirectionOrigin.rotation;
        float angle = context.DirectionOffset;
        Quaternion baseRot = Quaternion.LookRotation(context.AbilityDirectionOrigin.forward, Vector3.up);
        Quaternion finalRot = angle == 0f ? baseRot : Quaternion.AngleAxis(angle, context.AbilityDirectionOrigin.right) * baseRot;
        GameObject obj = _pool.GetFromPool(context.AbilityFireOrigin.position, finalRot) as GameObject;

        if (obj == null) return;
        ComponentRegistry.TryGet(obj, out IPoolable projectile);
       
        projectile?.LaunchPoolable();
    }

    public override void UpdatePool(PoolIdSO poolId, Action<bool> actionCompleteCallback = null)
    {
        if (_pool != null && poolId.Id == _poolId) return;
      
        _poolId = poolId.Id;
        IsReady = false;
        this.RequestPool(_poolId, poolCallback);
    }


    protected override void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (poolId != _poolId || pool == null) return;
        _pool = pool;
        IsReady = true;
    }


}
