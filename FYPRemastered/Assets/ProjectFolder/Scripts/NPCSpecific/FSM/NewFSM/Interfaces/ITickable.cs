

using System;

public interface ITickable
{
    //event Action<float> OnTick;
    void Tick(float dt);

    void LateTick(float dt);

    //Action<float> OnLateTick { get; }
}
