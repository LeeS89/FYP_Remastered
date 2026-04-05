using System.Collections.Generic;
using System.Threading.Tasks;


public abstract class FsmResources : IAddressableService, IFsmService
{
    public abstract void Dispose();

    public virtual float GetStoppingDistance() => 0f;
   

    public abstract Task<bool> TryInitialiseAsync(FeatureMeta data);

    protected virtual async Task<bool> TryLoadSubData(List<string> addressKeys) { await Task.CompletedTask; return false; }
   
}
