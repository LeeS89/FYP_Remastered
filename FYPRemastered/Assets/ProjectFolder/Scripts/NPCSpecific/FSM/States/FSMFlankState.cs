using Npc.API;
using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using Random = UnityEngine.Random;

public class FsmFlankState : FsmBaseState<FlankDeps>
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
    public FsmFlankState(FlankDeps deps, SharedFsmStateServices sharedDeps, IFsmStateEvents stateContext) 
        : base(deps, sharedDeps, stateContext, StateId.Flank)
    {
        //_deps = deps;
        _flankService = _deps.FlankService;
        _candidateDestinations.EnsureCapacity(25);
        _flankStepsToTry.EnsureCapacity(10);
        _onFlankCandidatesReceived = OnCandidatesReceived;
    }

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
                ReasonForDestinationCheck.ValidatePathForDestination,
                null,
                DestinationResult.CandidatesNullOrEmpty,
                Vector3.zero,
                StateId.Flank
            );
            base.OnProcessedDestinationsResult(in noPathResult);
            return;
        }

        Vector3 targetPos;
        if (!TryGetTargetPosition(out targetPos)) return; // Notify maybe

        // In DestinationResult, change found bool to result enum with values Found, NotFound, NoPrimaryTarget
        int stepsToTry = _flankStepsToTry[0];
        _flankStepsToTry.RemoveAt(0);
        _flankService?.TryGetFlankCandidates(/*_ownerData.PrimaryTarget.Position()*//*_deps.Target.Position()*/targetPos, stepsToTry, _candidateDestinations, _onFlankCandidatesReceived);
    }

    protected override void ValidateCandidateDestinations()
    {
        if (!_isInState || _candidateDestinations == null) return;
    }

    protected override void OnProcessedDestinationsResult(in DestinationResultInfo result)
    {
        throw new System.NotImplementedException();
    }

    private void OnCandidatesReceived(bool success)
    {
        if (!_isInState) return;
        // if ! success or no candidates, Send PathResult with found = false
    }

    private void SortStepsToTry()
    {
        _flankStepsToTry.Clear();

        int randomIndex = Random.Range(/*_ownerData.MinFlankSteps*/_deps.MinFlankSteps, /*_ownerData.MaxFlankSteps + 1*/_deps.MaxFlankSteps +1);

        int temp = randomIndex;
        while (temp >= _deps.MinFlankSteps) // 4 will eventually be changed to a passed minSteps parameter
        {
            _flankStepsToTry.Add(temp);
            temp--;
        }
        temp = randomIndex + 1;
        while (temp <= /*_ownerData.MaxFlankSteps*/_deps.MaxFlankSteps)
        {
            _flankStepsToTry.Add(temp);
            temp++;
        }
    }



}

public sealed class FlankDeps : FsmBaseState<FlankDeps>.FsmBaseStateDeps
{
    public IFlankService FlankService { get; private set; }
    public int MinFlankSteps { get; private set; }
    public int MaxFlankSteps { get; private set; }

    public FlankDeps(IFlankService flankService, IPathResolver resolver, FlankStateConfig config) : base(resolver)
    {
        FlankService = flankService;
        MinFlankSteps = config?.minFlankSteps ?? 4;
        MaxFlankSteps = config?.maxFlankSteps ?? 12;
    }

}
