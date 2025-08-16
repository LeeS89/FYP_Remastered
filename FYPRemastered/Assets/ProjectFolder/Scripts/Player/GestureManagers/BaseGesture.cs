using UnityEngine;

public abstract class BaseGesture : ComponentEvents
{

    public abstract void OnGestureRecognized();

    public abstract void OnGestureReleased();

   
    protected virtual void ResetStates() { }

    protected override void OnSceneStarted() { }


    protected override void OnSceneComplete() { }


    protected override void OnPlayerDeathStatusUpdated(bool isDead)
    {
        base.OnPlayerDeathStatusUpdated(isDead);
    }


   // protected override void OnPlayerRespawned() {  InputEnabled = true; }

    protected override void RegisterGlobalEvents() { }


    protected override void UnRegisterGlobalEvents() { }
    
}
