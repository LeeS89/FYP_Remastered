using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


#region Top level single instance Services where pre calculated scene data gets loaded and passed down to the bridge classes

public interface IFsmDataService { }

public interface IFsmControlService : IFsmDataService
{
    float GetSprintEnterDistance();
    float GetSprintExitDistance();
    float GetWalkSpeed();
    float GetSprintSpeed();
}


public interface IFsmStateData : IFsmDataService
{
    // For Data such as Stop distance
}

public interface IChaseService : IFsmStateData
{
    //bool TryGetDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer);
    bool TargetIsMoving(IInstanceIdentifiable id);
    bool TryGetSqrDistanceToTarget(IInstanceIdentifiable id, Vector3 from, out float sqrDistance);

    bool TryRegisterDistanceToTargetMonitoring(IInstanceIdentifiable id, Action<float> onDistanceUpdate, out float initialDistance);
    bool TryUnregisterDistanceToTargetMonitoring(IInstanceIdentifiable id);
}

public interface IPatrolService : IFsmStateData
{
    float GetIdleTimeSeconds();
    // bool TryGetWaypoints(object requester, List<Vector3> buffer);

    // bool TryReleaseWaypoints(object requester, List<Vector3> buffer);
}

public interface IFlankService : IFsmStateData
{
    void TryGetFlankCandidates(Vector3 flankTargetPos, int numSteps, List<Vector3> buffer, Action<bool> OnRequestComplete);
}
#endregion





#region Mid level Bridges between service and Fsm layers
public interface IFsmDestinationProvider
{
    bool TryGetDestinationCandidates(List<Vector3> buffer);
    void ReleaseCandidates(List<Vector3> buffer);
}

public interface IFsmDataProvider { }

public interface IFsmControlDataProvider : IFsmDataProvider
{
    float SprintEnterDistance { get; }
    float SprintExitDistance { get; }
    float WalkSpeed { get; }
    float SprintSpeed { get; }

}

public interface IFsmPatrolDataProvider : IFsmDataProvider { }
public interface IFsmChaseDataProvider : IFsmDataProvider
{
    bool TryRegisterDistanceMonitoring(IInstanceIdentifiable id, Vector3 currentPosition, /*ITargetable targetToCompare,*/ Action<float> callback, out float initDist);
    bool TryUnregisterDistanceMonitoring(IInstanceIdentifiable id);
    bool TargetIsMoving();
}

public interface IFsmFlankDataProvider : IFsmDataProvider { }

#endregion






#region Low level Fsm Communications between controller and states
public interface IFsmStateContext : IInstanceIdentifiable
{
    bool TryGetPath(out NavMeshPath path);
    bool TryGetCurrentPosition(out Vector3? currentPos);
    void ProcessDestinationResult(in DestinationResultInfo result);
    void RequestAnimation(AnimationCue cue, StateId id);
    void RequestRotation(float requestedAngle, StateId id, Action<bool> onComplete);
}

public interface IInstanceIdentifiable
{
    int EntityId { get; }
}


public interface ITargetProvider
{
    bool TryGetTarget(out ITargetable target);
    bool TryGetTargetPosition(out Vector3? position);
}
#endregion