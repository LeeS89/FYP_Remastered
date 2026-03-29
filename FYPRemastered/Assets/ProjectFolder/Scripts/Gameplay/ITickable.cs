

using System;

public interface ITickable : IDisposable
{
    //event Action<float> OnTick;
    //void Pause();
   // void Resume();
    void Tick(float dt);

    void LateTick(float dt);

    //Action<float> OnLateTick { get; }
}

public interface IDisposable// : ITickable
{
   // void Reset();
    void Dispose();
}
