using System;
using UnityEngine;

public interface IIntentState
{
    void Enter(NPCController self);
    void Exit(NPCController self);
    void Handle(NPCController self, StateNotification notification);
   

    //void Tick(NPCController self, float dt); => Optional for Later
}
