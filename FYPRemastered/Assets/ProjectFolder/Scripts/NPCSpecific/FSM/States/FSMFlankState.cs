using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using Random = UnityEngine.Random;

public class FSMFlankState : FSMBaseState
{
    private IFlankService _flankService;
    private Action _onFlankCandidatesReceived;
    private List<int> _flankStepsToTry = new();

    public FSMFlankState(IAgentData data, IPathResolver resolver, IFSMStateContext stateContext) 
        : base(data, resolver, stateContext, StateId.Flank)
    {
        _candidateDestinations.EnsureCapacity(25);
        _flankStepsToTry.EnsureCapacity(10);
        _onFlankCandidatesReceived = OnCandidatesReceived;
    }

    public override void EnterState()
    {
        base.EnterState();
        SortStepsToTry();
    }

    public override void RetrieveCandidateDestinations()
    {
        if (_flankStepsToTry.Count == 0)
        {
            DestinationResult noPathResult = new DestinationResult
            (
                ReasonForDestinationCheck.ValidatePathForDestination,
                null,
                false,
                Vector3.zero,
                StateId.Flank
            );
            base.OnPathResultReceived(in noPathResult);
            return;
        }
        // In DestinationResult, change found bool to result enum with values Found, NotFound, NoPrimaryTarget
        int stepsToTry = _flankStepsToTry[0];
        _flankStepsToTry.RemoveAt(0);
        _flankService?.TryGetFlankCandidates(_ownerData.PrimaryTarget.Position(), stepsToTry, _candidateDestinations, _onFlankCandidatesReceived);
    }

    public override void ValidateCandidateDestinations()
    {
        
    }

    protected override void OnPathResultReceived(in DestinationResult result)
    {
        throw new System.NotImplementedException();
    }

    private void OnCandidatesReceived()
    {

    }

    private void SortStepsToTry()
    {
        _flankStepsToTry.Clear();

        int randomIndex = Random.Range(_ownerData.MinFlankSteps, _ownerData.MaxFlankSteps + 1);

        int temp = randomIndex;
        while (temp >= 4) // 4 will eventually be changed to a passed minSteps parameter
        {
            _flankStepsToTry.Add(temp);
            temp--;
        }
        temp = randomIndex + 1;
        while (temp <= _ownerData.MaxFlankSteps)
        {
            _flankStepsToTry.Add(temp);
            temp++;
        }
    }
}
