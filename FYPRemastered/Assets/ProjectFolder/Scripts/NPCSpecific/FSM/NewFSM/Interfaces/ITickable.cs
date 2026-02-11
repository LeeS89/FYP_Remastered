

using System;

public interface ITickable : ILifecycle
{
    //event Action<float> OnTick;
    //void Pause();
   // void Resume();
    void Tick(float dt);

    void LateTick(float dt);

    //Action<float> OnLateTick { get; }
}

public interface ILifecycle// : ITickable
{
   // void Reset();
    void Dispose();
}
