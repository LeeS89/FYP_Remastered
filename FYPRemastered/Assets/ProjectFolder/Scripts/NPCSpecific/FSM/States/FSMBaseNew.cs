using System;
using System.Collections.Generic;
using UnityEngine;

public partial class FSMBaseNew : IFSMControl
{
    private IReadOnlyDictionary<StateId, IFSMState> _states;
    private IAgentData _ownerData;
    private IPathResolver _pathFinder;
    private IFieldOfViewRunner _fovHandler;
    public StateId CurrentState { get; } = StateId.None;

    public IFSMControl.OnNotifyOwner Notification { get; set; }
    public Action<AnimationCue> OnAnimationIntent { get; set; }
    public Action<Vector3> OnMapDestinationToZone { get; set; }

    #region Obsolete
    // Obsolete
    public Action<float> OnLateTick => throw new NotImplementedException();

    public void BeginChase(StateId id)
    {
        throw new NotImplementedException();
    }

    public void BeginFlank(StateId id)
    {
        throw new NotImplementedException();
    }

    public void BeginPatrol(StateId id)
    {
        throw new NotImplementedException();
    }
    public void TakeCover(StateId id)
    {
        throw new NotImplementedException();
    }
    public int? TryGetPatrolZone()
    {
        throw new NotImplementedException();
    }
    public void FollowGroup(StateId id)
    {
        throw new NotImplementedException();
    }
    // End Obsolete
    #endregion

    public void EnterState(StateId id)
    {
        if (_states != null && _states.TryGetValue(id, out var state))
            state.EnterState(id);
        // else => Notify state doesnt exist
    }

    public void ExitState(StateId id)
    {
        throw new NotImplementedException();
    }

  

    public bool IsMoving()
    {
        throw new NotImplementedException();
    }

    public void LateTick(float dt)
    {
        throw new NotImplementedException();
    }

    public void OnDestinationReached()
    {
        throw new NotImplementedException();
    }

   

    public void Tick(float dt)
    {
        throw new NotImplementedException();
    }

   
}
