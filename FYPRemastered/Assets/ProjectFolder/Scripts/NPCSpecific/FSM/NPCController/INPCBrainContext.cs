using UnityEngine;

public interface INPCBrainContext : ITargetable
{
    StateId CurrentFSMState { get; }
    CombatOrder CurrentOrder { get; }
    FOVResult CurrentFOVState { get; }
}
