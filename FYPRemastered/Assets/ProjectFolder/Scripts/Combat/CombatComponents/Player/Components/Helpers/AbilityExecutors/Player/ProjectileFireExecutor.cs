using System;
using UnityEngine;

public class ProjectileFireExecutor : IEffectExecutor
{
    private IPoolManager _pool;
    private EffectDef _def;
    private Action<PoolIdSO, IPoolManager> poolCallback;

    public ProjectileFireExecutor(EffectDef def = null)
    {
        if (def == null || def.PoolId == null) return;
        _def = def;
        poolCallback = OnPoolReceived;
        this.RequestPool(_def.PoolId, poolCallback);
    }

    public void Execute(in AbilityContext context, EffectDef def, CuePhase phase)
    {
        if (def.Kind != EffectKind.SpawnAndFire) return;
        if (_pool == null) return;

        Vector3 direction = context.GazeOrigin.forward;
        Quaternion rotation = context.GazeOrigin.rotation;
        GameObject obj = _pool.GetFromPool(context.CurrentOrigin.position, rotation) as GameObject;
        ComponentRegistry.TryGet(obj, out IPoolable projectile);

        projectile?.LaunchPoolable();
    }

    private void OnPoolReceived(PoolIdSO poolId, IPoolManager pool)
    {
        if (poolId != _def.PoolId || pool == null) return;
        _pool = pool;
    }
}
