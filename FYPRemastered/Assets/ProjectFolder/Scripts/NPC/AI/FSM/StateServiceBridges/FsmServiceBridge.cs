using UnityEngine;

public abstract class FsmServiceBridge<TService> : IFsmData
{
    protected readonly TService _service;
    public FsmServiceBridge(TService service) => _service = service;
}
