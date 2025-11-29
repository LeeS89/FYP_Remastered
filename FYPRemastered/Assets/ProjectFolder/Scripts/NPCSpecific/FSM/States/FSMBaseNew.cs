using System;
using System.Collections.Generic;
using UnityEngine;

public partial class FSMBaseNew : IFSMControl
{
    private IReadOnlyDictionary<StateId, IFSMState> _states;
    private IAgentData _ownerData;
    private IPathResolver _pathFinder;
    private IFieldOfViewRunner _fovHandler;

    private IFSMState _current;
    public StateId CurrentStateId => _current?.Id ?? StateId.None;

    public IFSMControl.OnNotifyOwner Notification { get; set; }
    public Action<AnimationCue> OnAnimationIntent { get; set; }
    public Action<Vector3> OnMapDestinationToZone { get; set; }

    private bool _isInStateTransition = false;

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

   
    public void SwitchTo(StateId next)
    {
        if (next == CurrentStateId || next == StateId.None) return;


        if(_states != null && _states.TryGetValue(next, out var nextstate))
        {
            _current?.ExitState();
            _current = nextstate;
            _current.EnterState();
        }   // else => Notify state doesnt exist
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
