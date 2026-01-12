using System;
using UnityEngine;

public interface INpcAnimationControl
{
    void PlayClip(AnimationCue cue);
    void Tick(Vector3 velocity, Vector3 forward);
    void SetIKLookTarget(Transform target);
    void IkLookAtTarget(bool look);
    bool IsAnimationLayerActive(AnimationLayer layer);
    void ToggleAnimationLayer(AnimationLayer layer, bool activate, Action completedCB = null);

    void SetLookAt(bool look, Transform target = null);
}
