
using System;

[Obsolete]
public interface IPlayerEvents
{
    public void OnSceneStarted();

    public void OnSceneComplete();

    public void OnPlayerDied();

    public void OnPlayerRespawned();
}
