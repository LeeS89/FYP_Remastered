using UnityEngine;

public interface INPCBrainContext : ITargetable
{
    StateId CurrentFSMState { get; }
    CombatOrder CurrentComOrder { get; }
    RotationOrder CurrentRotOrder { get; }
    FOVResult CurrentFOVState { get; }
}
