using UnityEngine;
using UnityEngine.AI;

public readonly struct DestinationResultInfo
{

    public readonly ReasonForDestinationCheck RequestReason;
    public readonly NavMeshPath Path;
    public readonly DestinationResult Result;
    public readonly Vector3 Destination;
    public readonly StateId Id;

    public DestinationResultInfo(ReasonForDestinationCheck reason, NavMeshPath path, DestinationResult result, Vector3 dest, StateId id)
    {
        RequestReason = reason;
        Path = path;
        Id = id;
        Result = result;
        Destination = dest;
    }

}
