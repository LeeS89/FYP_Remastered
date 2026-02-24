using System;
using UnityEngine;

[Obsolete("", true)]
public interface IAnimationCueSourceObsolete
{
    Action<AnimationCue> OnAnimationIntent { get; set; }
}
