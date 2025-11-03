using System;
using UnityEngine;

public interface IIntentState
{
    void Enter(IFSMOwner self);
    void Exit(IFSMOwner self);
    void Handle(IFSMOwner self, NotifyOwnerNPC notification);

    StateId Id { get; }

    //void Tick(NPCController self, float dt); => Optional for Later
}
