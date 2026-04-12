using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public interface IDestinationResolver
{
  
   // void CancelAll();

    //void ProcessDestinationCandidates(StateId id, ReasonForDestinationCheck reason, List<Vector3> candidates, NavMeshPath path, Vector3 fromPos, DestinationResultCallback callBack);
 //   void ProcessDestinationCandidates(in DestinationRequest request);

    Task<DestinationResultInfo> ProcessCandidates(in DestinationRequest request);

    void CancelAll();
}
