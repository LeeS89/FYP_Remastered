using UnityEngine;

public class ProjectileFireExecutor : IEffectExecutor
{
    public void Execute(in AbilityContext context, EffectDef def, CuePhase phase)
    {
        if (def.Kind != EffectKind.SpawnAndFire) return;
    }
}
