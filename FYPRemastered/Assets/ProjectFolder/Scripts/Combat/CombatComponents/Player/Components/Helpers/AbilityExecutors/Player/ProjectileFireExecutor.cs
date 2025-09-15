using System;
using UnityEngine;

public class ProjectileFireExecutor : IEffectExecutor
{
    private IPoolManager _pool;
    private EffectDef _def;
    private Action<string, IPoolManager> poolCallback;

    public ProjectileFireExecutor(EffectDef def = null)
    {
        if (def == null || def.PoolId == null) return;
        _def = def;
        poolCallback = OnPoolReceived;
        this.RequestPool(_def.PoolId.Id, poolCallback);
    }

    public void Execute(in AbilityContext context, EffectDef def, CuePhase phase)
    {
        if (def.Kind != EffectKind.SpawnAndFire) return;
        if (_pool == null) return;

        Vector3 direction = context.GazeOrigin.forward;
        Quaternion rotation = context.GazeOrigin.rotation;
        Quaternion rot = Quaternion.LookRotation(context.GazeOrigin.forward);
        GameObject obj = _pool.GetFromPool(context.CurrentOrigin.position, rot) as GameObject;

        if (obj == null) return;
        ComponentRegistry.TryGet(obj, out IPoolable projectile);
       
        projectile?.LaunchPoolable();
    }

    private void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (poolId != _def.PoolId.Id || pool == null) return;
        _pool = pool;
    }
}
