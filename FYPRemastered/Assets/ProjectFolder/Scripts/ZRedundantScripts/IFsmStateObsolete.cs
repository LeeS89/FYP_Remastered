using System;
using UnityEngine;

[Obsolete("", true)]
public interface IFsmStateObsolete : ITickable
{
    void EnterState();

    void ExitState();

    void OnDestinationReached();

    void OnDestinationSet();

    // void ValidateCandidateDestinations();

    void TryRepath();

    bool NeedsNewPath();
    //void RetrieveCandidateDestinations();

   // bool UsesRandomAgentStopDistance { get; }
    StateId GetId();

    float GetDesiredStoppingDistance();
}

[Obsolete("", true)]
public interface IFsmStateNew : ITickable
{
    

    void EnterState();

    void ExitState();

    void OnDestinationReached();

    void OnDestinationSet();

    // void ValidateCandidateDestinations();

    void TryRepath();

    bool NeedsNewPath();
    //void RetrieveCandidateDestinations();

    // bool UsesRandomAgentStopDistance { get; }
    StateId GetId();

    float GetDesiredStoppingDistance();
}

[Obsolete("", true)]
public interface IFsmStateNew<TService/*, TContext*/> : IFsmStateNew where TService : IFsmStateService// where TContext : IContext //, ITickable
{
    // TContext Context { get; }
    /* TContext Context { get; }

     void EnterState();

     void ExitState();

     void OnDestinationReached();

     void OnDestinationSet();

     // void ValidateCandidateDestinations();

     void TryRepath();

     bool NeedsNewPath();
     //void RetrieveCandidateDestinations();

    // bool UsesRandomAgentStopDistance { get; }
     StateId GetId();

     float GetDesiredStoppingDistance();*/
}




