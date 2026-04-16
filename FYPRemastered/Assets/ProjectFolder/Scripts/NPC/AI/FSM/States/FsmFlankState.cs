using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class FsmFlankState : FsmBaseState<IFsmFlankData>
{
    // private IFlankDeps _deps;
    private IFlankService _flankService;
    private Action<bool> _onFlankCandidatesReceived;
    private List<int> _flankStepsToTry = new();

    /* public FSMFlankState(IFlankService flankService, IAgentData data, IPathResolver resolver, IFSMStateContext stateContext) 
         : base(data, resolver, stateContext, StateId.Flank)
     {
         _flankService = flankService;
         _candidateDestinations.EnsureCapacity(25);
         _flankStepsToTry.EnsureCapacity(10);
         _onFlankCandidatesReceived = OnCandidatesReceived;
     }*/
    /*  public FsmFlankStateNewest(FlankDeps deps, SharedFsmStateServices sharedDeps, IFsmStateEventsObsoleteO stateContext)
          : base(deps, sharedDeps, stateContext, StateId.Flank)
      {
          //_deps = deps;
          _flankService = _deps.FlankService;
          _candidateDestinations.EnsureCapacity(25);
          _flankStepsToTry.EnsureCapacity(10);
          _onFlankCandidatesReceived = OnCandidatesReceived;
      }*/


    public FsmFlankState(/*IFsmStateContext stateController,*/ IFsmFlankData dataP, IDestinationResolver pathResolver, ICoroutineHost host)
        : base(/*stateController,*/ dataP, pathResolver, host, StateId.Flank) { _candidateDestinations.EnsureCapacity(15); _onFlankCandidatesReceived = OnCandidatesReceived; }


    public override void EnterState()
    {
        SortStepsToTry();
        base.EnterState();
    }

    protected override void RetrieveCandidateDestinations()
    {
        if (!_isInState || _candidateDestinations == null) return;

        if (_flankStepsToTry.Count == 0) // Add in arbitrary steps, 5 to 8 etc.
        {
            DestinationResultInfo noPathResult = new DestinationResultInfo
            (
                DestinationRequestReason.ValidatePathForDestination,
                null,
                DestinationResult.CandidatesNullOrEmpty,
                Vector3.zero,
                StateId.Flank
            );
            base.OnProcessedDestinationsResult(in noPathResult);
            return;
        }

        /* Vector3 targetPos;
         if (!TryGetTargetPosition(out targetPos)) return; // Notify maybe

         // In DestinationResult, change found bool to result enum with values Found, NotFound, NoPrimaryTarget
         int stepsToTry = _flankStepsToTry[0];
         _flankStepsToTry.RemoveAt(0);
         _flankService?.TryGetFlankCandidates(*//*_ownerData.PrimaryTarget.Position()*//*_deps.Target.Position()*//*targetPos, stepsToTry, _candidateDestinations, _onFlankCandidatesReceived);*/
    }

    protected /*override*/ void ValidateAndSendCandidateDestinations()
    {
        if (!_isInState || _candidateDestinations == null) return;
    }

 

    private void OnCandidatesReceived(bool success)
    {
        if (!_isInState) return;
        // if ! success or no candidates, Send PathResult with found = false
    }

    private void SortStepsToTry()
    {
        _flankStepsToTry.Clear();

        //  int randomIndex = Random.Range(/*_ownerData.MinFlankSteps*/_deps.MinFlankSteps, /*_ownerData.MaxFlankSteps + 1*/_deps.MaxFlankSteps + 1);

        /*   int temp = randomIndex;
           while (temp >= _deps.MinFlankSteps) // 4 will eventually be changed to a passed minSteps parameter
           {
               _flankStepsToTry.Add(temp);
               temp--;
           }
           temp = randomIndex + 1;
           while (temp <= *//*_ownerData.MaxFlankSteps*//*_deps.MaxFlankSteps)
           {
               _flankStepsToTry.Add(temp);
               temp++;
           }*/
    }


}
