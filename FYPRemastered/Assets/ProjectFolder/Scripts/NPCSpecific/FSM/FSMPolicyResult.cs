using UnityEngine;

public readonly struct FSMPolicyResult
{
    public readonly bool PathBlocked;
    public readonly bool PathToPrimaryBlocked;
    public readonly bool DestinationReached;
    public readonly FSMPolicy CurrentPolicy;
    public readonly uint Version;

    public FSMPolicyResult(FSMPolicy currentPolicy, bool pathBlocked, bool pathToPrimaryBlocked, bool destinationReached)
    {
        CurrentPolicy = currentPolicy;
        PathBlocked = pathBlocked;
        PathToPrimaryBlocked = pathToPrimaryBlocked;
        DestinationReached = destinationReached;
        this.Version = currentPolicy.Version;
    }
}
