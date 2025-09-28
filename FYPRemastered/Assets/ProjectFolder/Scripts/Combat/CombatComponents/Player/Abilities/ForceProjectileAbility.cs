using UnityEngine;

public class ForceProjectileAbility : AbilityBase
{
    public ForceProjectileAbility(AbilityDefinition def/*, EventManager eventManager*/) : base(def/*, eventManager*/)
        => InitializePools();


    public override bool TryActivate(float now, AbilityOrigins origin = null)
    {
        if (!CanActivate()) return false;

        ExecuteOrigin = origin?.Position;
        ExecuteDirectionOrigin = origin?.DirectionOrigin;
        DirectionOffset = origin.DirectionOffset;

        return true;
    }

    public override void Tick(float now)
    {
        if (!_channeling || now < _nextTick) return;
        if (!HasSufficientResources())
        {
            End();
            return;
        }
    }

    public override void End()
    {
        
    }

    protected override void Execute(CuePhase phase, Transform origin, Vector3? direction = null, IPoolManager pool = null)
    {
        if(phase != CuePhase.End)
        {
           // Quaternion baseRotation = Quaternion.LookRotation(direction.Value)
        }
    }

    protected override bool CanActivate()
    {
        return base.CanActivate();
    }

    protected override bool HasSufficientResources()
    {
        return base.HasSufficientResources();
    }

    protected override void SpendResources()
    {
        
    }

    protected override void OnInterrupted()
    {
        
    }


}
