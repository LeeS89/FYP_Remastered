using System;
using UnityEngine;

[Obsolete]
public interface IIntentStateObsolete
{
    void Enter(IFSMOwner self);
    void Exit(IFSMOwner self);
    void Handle(IFSMOwner self, NPCNotification notification);

    StateId Id { get; }

    //void Tick(NPCController self, float dt); => Optional for Later
}
