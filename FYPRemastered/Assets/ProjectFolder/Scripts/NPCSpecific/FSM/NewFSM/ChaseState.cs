using UnityEngine;

public sealed class ChaseState : IntentStateBase
{
    public static readonly ChaseState Instance = new();
    private ChaseState() { }

    public override void Enter(NPCController self)
    {
        throw new System.NotImplementedException();
    }

    public override void Handle(NPCController self, PolicyNotification notification)
    {
        switch (notification)
        {
            
        }
    }
}
