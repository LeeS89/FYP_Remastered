using UnityEngine;

public abstract class BaseGesture : ComponentInit<IPlaceholderService, PlayerEventManager>/*ComponentEvents*/
{

    public abstract void OnGestureRecognized();

    public abstract void OnGestureReleased();

   
    protected virtual void ResetStates() { }

  //  protected override void OnSceneStarted() { }


  //  protected override void OnSceneComplete() { }


  /*  protected override void OnPlayerDeathStatusUpdated(bool isDead)
    {
        base.OnPlayerDeathStatusUpdated(isDead);
    }*/


    
}
