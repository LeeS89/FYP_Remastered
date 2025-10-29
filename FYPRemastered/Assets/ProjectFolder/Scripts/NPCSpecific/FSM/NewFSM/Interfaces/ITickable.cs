

using System;

public interface ITickable
{
    Action<float> Tick { get; }
}
