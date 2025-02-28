using UnityEngine;

public abstract class BaseGesture : MonoBehaviour
{
    public abstract void OnGestureRecognized();
    public abstract void OnGestureReleased();
}
