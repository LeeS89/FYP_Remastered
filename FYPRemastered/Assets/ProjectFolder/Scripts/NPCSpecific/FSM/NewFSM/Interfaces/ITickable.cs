

using System;

public interface ITickable
{
    Action<float> Tick { get; }

    Action<float> LateTick { get; }
}
