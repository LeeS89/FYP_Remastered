using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IPathResolver
{
  
    void CancelAll();

    //void ProcessDestinationCandidates(StateId id, ReasonForDestinationCheck reason, List<Vector3> candidates, NavMeshPath path, Vector3 fromPos, DestinationResultCallback callBack);
    void ProcessDestinationCandidates(in DestinationRequest request);

}
