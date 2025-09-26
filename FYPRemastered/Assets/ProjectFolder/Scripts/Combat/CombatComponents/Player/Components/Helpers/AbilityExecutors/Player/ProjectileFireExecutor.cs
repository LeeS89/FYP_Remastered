using System;
using UnityEngine;

public class ProjectileFireExecutor : ExecutorBase
{
  
    public ProjectileFireExecutor(Action spendCallback) : base(spendCallback) { }
     

    public override void Execute(in AbilityContext context, EffectDef def, CuePhase phase, IPoolManager pool = null)
    {
  
        if (def.Kind != EffectKind.SpawnAndFire || pool == null) return;
 

        if (phase != CuePhase.End/*phase == CuePhase.Start || phase == CuePhase.Impact*/)
        {
            // Vector3 direction = context.AbilitydirectionOrigin.forward;
            Quaternion rotation = context.AbilityDirectionOrigin.rotation;
            float angle = context.DirectionOffset;
            Quaternion baseRot = Quaternion.LookRotation(context.AbilityDirectionOrigin.forward, Vector3.up);
            Quaternion finalRot = angle == 0f ? baseRot : Quaternion.AngleAxis(angle, context.AbilityDirectionOrigin.right) * baseRot;
            GameObject obj = pool.GetFromPool(context.AbilityFireOrigin.position, finalRot) as GameObject;

            if (obj == null) return;
            ComponentRegistry.TryGet(obj, out IPoolable projectile);

            if (projectile == null) return;
            spendResourcesNotification?.Invoke();
            projectile.LaunchPoolable();
        }
    }

    
}
