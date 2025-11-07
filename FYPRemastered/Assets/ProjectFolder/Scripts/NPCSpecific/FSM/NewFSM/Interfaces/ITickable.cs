

using System;

public interface ITickable
{
    //event Action<float> OnTick;
    void Tick(float dt);

    Action<float> LateTick { get; }
}
