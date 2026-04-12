using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public readonly struct DestinationRequest
{
    public readonly StateId StateId;
    public readonly Vector3 From;
    public readonly List<Vector3> Candidates;
    public readonly NavMeshPath Path;
    public readonly DestinationRequestReason Reason;
    [Obsolete]
    public readonly DestinationResultCallback Callback;

    public DestinationRequest(StateId id, Vector3 currentPosition, List<Vector3> candidates, NavMeshPath path, DestinationRequestReason reason, DestinationResultCallback cb = null)
        => (StateId, From, Candidates, Path, Reason, Callback) = (id, currentPosition, candidates, path, reason, cb);

}

[Obsolete]
public delegate void DestinationResultCallback(in DestinationResultInfo result);
