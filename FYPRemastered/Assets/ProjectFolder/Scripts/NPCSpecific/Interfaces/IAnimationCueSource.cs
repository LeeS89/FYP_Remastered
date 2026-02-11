using System;
using UnityEngine;

public interface IAnimationCueSource
{
    Action<AnimationCue> OnAnimationIntent { get; set; }
}
